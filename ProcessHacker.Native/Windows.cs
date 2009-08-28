﻿/*
 * Process Hacker - 
 *   system-related functions
 *
 * Copyright (C) 2009 Flavio Erlich
 * Copyright (C) 2009 wj32
 * 
 * This file is part of Process Hacker.
 * 
 * Process Hacker is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Process Hacker is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Process Hacker.  If not, see <http://www.gnu.org/licenses/>.
 */

// 'member' is obsolete: 'text'
#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using ProcessHacker.Common;
using ProcessHacker.Native.Api;
using ProcessHacker.Native.Objects;
using ProcessHacker.Native.Security;

namespace ProcessHacker.Native
{
    /// <summary>
    /// Provides methods for manipulating the operating system.
    /// </summary>
    public static class Windows
    {
        public delegate bool EnumKernelModulesDelegate(KernelModule kernelModule);

        public delegate string GetProcessNameCallback(int pid);

        public static GetProcessNameCallback GetProcessName;

        /// <summary>
        /// A cache for type names; QuerySystemInformation with ALL_TYPES_INFORMATION fails for some 
        /// reason. The dictionary relates object type numbers to their names.
        /// </summary>
        internal static Dictionary<byte, string> ObjectTypes = new Dictionary<byte, string>();

        [ThreadStatic]
        private static MemoryAlloc _handlesBuffer;
        [ThreadStatic]
        private static MemoryAlloc _kernelModulesBuffer;
        [ThreadStatic]
        private static MemoryAlloc _processesBuffer;
        [ThreadStatic]
        private static MemoryAlloc _servicesBuffer;

        private static int _numberOfProcessors = 0;
        private static int _pageSize = 0;
        private static IntPtr _kernelBase = IntPtr.Zero;
        private static string _kernelFileName = null;
        private static KernelModule _kernelModule;

        public static int NumberOfProcessors
        {
            get
            {
                if (_numberOfProcessors == 0)
                    _numberOfProcessors = GetBasicInformation().NumberOfProcessors;

                return _numberOfProcessors;
            }
        }

        public static int PageSize
        {
            get
            {
                if (_pageSize == 0)
                    _pageSize = GetBasicInformation().PageSize;

                return _pageSize;
            }
        }

        public static IntPtr KernelBase
        {
            get
            {
                if (_kernelBase == IntPtr.Zero)
                    _kernelBase = GetKernelBase();

                return _kernelBase;
            }
        }

        public static string KernelFileName
        {
            get
            {
                if (_kernelFileName == null)
                    _kernelFileName = GetKernelFileName();

                return _kernelFileName;
            }
        }

        public static int BytesToPages(int bytes)
        {
            return (bytes - 1) / PageSize + 1;
        }

        public static void EnumKernelModules(EnumKernelModulesDelegate enumCallback)
        {
            NtStatus status;
            int retLength;

            if (_kernelModulesBuffer == null)
                _kernelModulesBuffer = new MemoryAlloc(0x1000);

            status = Win32.NtQuerySystemInformation(
                SystemInformationClass.SystemModuleInformation,
                _kernelModulesBuffer,
                _kernelModulesBuffer.Size,
                out retLength
                );

            if (status == NtStatus.InfoLengthMismatch)
            {
                _kernelModulesBuffer.Resize(retLength);

                status = Win32.NtQuerySystemInformation(
                    SystemInformationClass.SystemModuleInformation,
                    _kernelModulesBuffer,
                    _kernelModulesBuffer.Size,
                    out retLength
                    );
            }

            if (status >= NtStatus.Error)
                Win32.ThrowLastError(status);

            RtlProcessModules modules = _kernelModulesBuffer.ReadStruct<RtlProcessModules>();

            for (int i = 0; i < modules.NumberOfModules; i++)
            {
                var module = _kernelModulesBuffer.ReadStruct<RtlProcessModuleInformation>(RtlProcessModules.ModulesOffset, i);
                var moduleInfo = new Debugging.ModuleInformation(module);

                if (!enumCallback(new KernelModule(
                    moduleInfo.BaseAddress,
                    moduleInfo.Size,
                    moduleInfo.Flags,
                    moduleInfo.BaseName,
                    FileUtils.FixPath(moduleInfo.FileName)
                    )))
                    break;
            }
        }

        public static SystemBasicInformation GetBasicInformation()
        {
            NtStatus status;
            SystemBasicInformation sbi;
            int retLength;

            if ((status = Win32.NtQuerySystemInformation(
                SystemInformationClass.SystemBasicInformation,
                out sbi,
                Marshal.SizeOf(typeof(SystemBasicInformation)),
                out retLength
                )) >= NtStatus.Error)
                Win32.ThrowLastError(status);

            return sbi;
        }

        /// <summary>
        /// Enumerates the handles opened by every running process.
        /// </summary>
        /// <returns>An array containing information about the handles.</returns>
        public static SystemHandleEntry[] GetHandles()
        {
            int retLength = 0;
            int handleCount = 0;
            SystemHandleEntry[] returnHandles;

            if (_handlesBuffer == null)
                _handlesBuffer = new MemoryAlloc(0x1000);

            MemoryAlloc data = _handlesBuffer;

            NtStatus status;

            // This is needed because NtQuerySystemInformation with SystemHandleInformation doesn't 
            // actually give a real return length when called with an insufficient buffer. This code 
            // tries repeatedly to call the function, doubling the buffer size each time it fails.
            while ((status = Win32.NtQuerySystemInformation(
                SystemInformationClass.SystemHandleInformation,
                data,
                data.Size,
                out retLength)
                ) == NtStatus.InfoLengthMismatch)
            {
                data.Resize(data.Size * 2);

                // Fail if we've resized it to over 16MB - protect from infinite resizing
                if (data.Size > 16 * 1024 * 1024)
                    throw new OutOfMemoryException();
            }

            if (status >= NtStatus.Error)
                Win32.ThrowLastError(status);

            // The structure of the buffer is the handle count plus an array of SYSTEM_HANDLE_INFORMATION 
            // structures.
            handleCount = data.ReadStruct<SystemHandleInformation>().NumberOfHandles;
            returnHandles = new SystemHandleEntry[handleCount];

            for (int i = 0; i < handleCount; i++)
            {
                returnHandles[i] = data.ReadStruct<SystemHandleEntry>(SystemHandleInformation.HandlesOffset, i);
            }

            return returnHandles;
        }

        /// <summary>
        /// Gets the base address of the currently running kernel.
        /// </summary>
        /// <returns>The kernel's base address.</returns>
        private static IntPtr GetKernelBase()
        {
            IntPtr kernelBase = IntPtr.Zero;

            Windows.EnumKernelModules((module) =>
            {
                kernelBase = module.BaseAddress;
                return false;
            });

            return kernelBase;
        }

        /// <summary>
        /// Gets the file name of the currently running kernel.
        /// </summary>
        /// <returns>The kernel file name.</returns>
        private static string GetKernelFileName()
        {
            string kernelFileName = null;

            EnumKernelModules((module) =>
            {
                kernelFileName = module.FileName;
                return false;
            });

            return kernelFileName;
        }

        public static KernelModule[] GetKernelModules()
        {
            List<KernelModule> kernelModules = new List<KernelModule>();

            EnumKernelModules((kernelModule) =>
            {
                kernelModules.Add(kernelModule);
                return true;
            });

            return kernelModules.ToArray();
        }

        public static Dictionary<int, List<NetworkConnection>> GetNetworkConnections()
        {
            var retDict = new Dictionary<int, List<NetworkConnection>>();
            int length = 0;

            Win32.GetExtendedTcpTable(IntPtr.Zero, ref length, false, 2, TcpTableClass.OwnerPidAll, 0);

            using (var mem = new MemoryAlloc(length))
            {
                if (Win32.GetExtendedTcpTable(mem, ref length, false, 2,
                    TcpTableClass.OwnerPidAll, 0) != 0)
                    Win32.ThrowLastError();

                int count = mem.ReadInt32(0);

                for (int i = 0; i < count; i++)
                {
                    var struc = mem.ReadStruct<MibTcpRowOwnerPid>(sizeof(int), i);

                    if (!retDict.ContainsKey(struc.OwningProcessId))
                        retDict.Add(struc.OwningProcessId, new List<NetworkConnection>());

                    retDict[struc.OwningProcessId].Add(new NetworkConnection()
                    {
                        Protocol = NetworkProtocol.Tcp,
                        Local = new IPEndPoint(struc.LocalAddress, ((ushort)struc.LocalPort).SwapBytes()),
                        Remote = new IPEndPoint(struc.RemoteAddress, ((ushort)struc.RemotePort).SwapBytes()),
                        State = struc.State,
                        Pid = struc.OwningProcessId
                    });
                }
            }

            Win32.GetExtendedUdpTable(IntPtr.Zero, ref length, false, 2, UdpTableClass.OwnerPid, 0);

            using (var mem = new MemoryAlloc(length))
            {
                if (Win32.GetExtendedUdpTable(mem, ref length, false, 2, UdpTableClass.OwnerPid, 0) != 0)
                    Win32.ThrowLastError();

                int count = mem.ReadInt32(0);

                for (int i = 0; i < count; i++)
                {
                    var struc = mem.ReadStruct<MibUdpRowOwnerPid>(sizeof(int), i);

                    if (!retDict.ContainsKey(struc.OwningProcessId))
                        retDict.Add(struc.OwningProcessId, new List<NetworkConnection>());

                    retDict[struc.OwningProcessId].Add(
                        new NetworkConnection()
                        {
                            Protocol = NetworkProtocol.Udp,
                            Local = new IPEndPoint(struc.LocalAddress, ((ushort)struc.LocalPort).SwapBytes()),
                            Pid = struc.OwningProcessId
                        });
                }
            }

            return retDict;
        }

        public static SystemPagefile[] GetPagefiles()
        {
            int retLength;
            List<SystemPagefile> pagefiles = new List<SystemPagefile>();

            using (MemoryAlloc data = new MemoryAlloc(0x200))
            {
                NtStatus status;

                while ((status = Win32.NtQuerySystemInformation(
                    SystemInformationClass.SystemPageFileInformation,
                    data,
                    data.Size,
                    out retLength)
                    ) == NtStatus.InfoLengthMismatch)
                {
                    data.Resize(data.Size * 2);

                    // Fail if we've resized it to over 16MB - protect from infinite resizing
                    if (data.Size > 16 * 1024 * 1024)
                        throw new OutOfMemoryException();
                }

                if (status >= NtStatus.Error)
                    Win32.ThrowLastError(status);

                pagefiles = new List<SystemPagefile>(2);

                int i = 0;
                SystemPagefileInformation currentPagefile;

                do
                {
                    currentPagefile = data.ReadStruct<SystemPagefileInformation>(i, 0);

                    pagefiles.Add(new SystemPagefile(
                        currentPagefile.TotalSize,
                        currentPagefile.TotalInUse,
                        currentPagefile.PeakUsage,
                        FileUtils.FixPath(currentPagefile.PageFileName.Read())
                        ));

                    i += currentPagefile.NextEntryOffset;
                } while (currentPagefile.NextEntryOffset != 0);

                return pagefiles.ToArray();
            }
        }

        public static Dictionary<int, SystemProcess> GetProcesses()
        {
            return GetProcesses(false);
        }

        /// <summary>
        /// Gets a dictionary containing the currently running processes.
        /// </summary>
        /// <returns>A dictionary, indexed by process ID.</returns>
        public static Dictionary<int, SystemProcess> GetProcesses(bool getThreads)
        {
            int retLength;
            Dictionary<int, SystemProcess> returnProcesses;

            if (_processesBuffer == null)
                _processesBuffer = new MemoryAlloc(0x10000);

            MemoryAlloc data = _processesBuffer;

            NtStatus status;
            int attempts = 0;

            while (true)
            {
                attempts++;

                if ((status = Win32.NtQuerySystemInformation(
                    SystemInformationClass.SystemProcessInformation,
                    data,
                    data.Size,
                    out retLength
                    )) >= NtStatus.Error)
                {
                    if (attempts > 3)
                        Win32.ThrowLastError(status);

                    data.Resize(retLength);
                }
                else
                {
                    break;
                }
            }

            returnProcesses = new Dictionary<int, SystemProcess>(32); // 32 processes on a computer?

            int i = 0;
            SystemProcess currentProcess = new SystemProcess();

            do
            {
                currentProcess.Process = data.ReadStruct<SystemProcessInformation>(i, 0);
                currentProcess.Name = currentProcess.Process.ImageName.Read();

                if (getThreads &&
                    currentProcess.Process.ProcessId != 0)
                {
                    currentProcess.Threads = new Dictionary<int, SystemThreadInformation>();

                    for (int j = 0; j < currentProcess.Process.NumberOfThreads; j++)
                    {
                        var thread = data.ReadStruct<SystemThreadInformation>(i +
                            Marshal.SizeOf(typeof(SystemProcessInformation)), j);

                        currentProcess.Threads.Add(thread.ClientId.ThreadId, thread);
                    }
                }

                returnProcesses.Add(currentProcess.Process.ProcessId, currentProcess);

                i += currentProcess.Process.NextEntryOffset;
            } while (currentProcess.Process.NextEntryOffset != 0);

            return returnProcesses;
        }

        public static Dictionary<int, SystemThreadInformation> GetProcessThreads(int pid)
        {
            int retLength;

            if (_processesBuffer == null)
                _processesBuffer = new MemoryAlloc(0x10000);

            MemoryAlloc data = _processesBuffer;

            NtStatus status;
            int attempts = 0;

            while (true)
            {
                attempts++;

                if ((status = Win32.NtQuerySystemInformation(SystemInformationClass.SystemProcessInformation, data.Memory,
                    data.Size, out retLength)) >= NtStatus.Error)
                {
                    if (attempts > 3)
                        Win32.ThrowLastError(status);

                    data.Resize(retLength);
                }
                else
                {
                    break;
                }
            }

            int i = 0;
            SystemProcessInformation process;

            do
            {
                process = data.ReadStruct<SystemProcessInformation>(i, 0);

                if (process.ProcessId == pid)
                {
                    var threads = new Dictionary<int, SystemThreadInformation>();

                    for (int j = 0; j < process.NumberOfThreads; j++)
                    {
                        var thread = data.ReadStruct<SystemThreadInformation>(i +
                            Marshal.SizeOf(typeof(SystemProcessInformation)), j);

                        threads.Add(thread.ClientId.ThreadId, thread);
                    }

                    return threads;
                }

                i += process.NextEntryOffset;

            } while (process.NextEntryOffset != 0);

            return null;
        }

        public static Dictionary<string, EnumServiceStatusProcess> GetServices()
        {
            using (ServiceManagerHandle manager =
                new ServiceManagerHandle(ScManagerAccess.EnumerateService))
            {
                int requiredSize;
                int servicesReturned;
                int resume = 0;

                if (_servicesBuffer == null)
                    _servicesBuffer = new MemoryAlloc(0x10000);

                MemoryAlloc data = _servicesBuffer;

                if (!Win32.EnumServicesStatusEx(manager, IntPtr.Zero, ServiceQueryType.Win32 | ServiceQueryType.Driver,
                    ServiceQueryState.All, data,
                    data.Size, out requiredSize, out servicesReturned,
                    ref resume, null))
                {
                    // resize buffer
                    data.Resize(requiredSize);

                    if (!Win32.EnumServicesStatusEx(manager, IntPtr.Zero, ServiceQueryType.Win32 | ServiceQueryType.Driver,
                        ServiceQueryState.All, data,
                        data.Size, out requiredSize, out servicesReturned,
                        ref resume, null))
                        Win32.ThrowLastError();
                }

                var dictionary = new Dictionary<string, EnumServiceStatusProcess>(servicesReturned);

                for (int i = 0; i < servicesReturned; i++)
                {
                    var service = data.ReadStruct<EnumServiceStatusProcess>(i);

                    dictionary.Add(service.ServiceName, service);
                }

                return dictionary;
            }
        }

        public static long GetTickCount()
        {
            // Read the tick count multiplier.
            int tickCountMultiplier = Marshal.ReadInt32(Win32.UserSharedData.Increment(
                KUserSharedData.TickCountMultiplierOffset));

            // Read the tick count.
            var tickCount = QueryKSystemTime(Win32.UserSharedData.Increment(
                KUserSharedData.TickCountOffset));

            return (((long)tickCount.LowPart * tickCountMultiplier) >> (int)24) +
                (((long)tickCount.HighPart * tickCountMultiplier) << (int)8);
        }

        public static SystemTimeOfDayInformation GetTimeOfDay()
        {
            NtStatus status;
            SystemTimeOfDayInformation timeOfDay;
            int retLength;

            status = Win32.NtQuerySystemInformation(
                SystemInformationClass.SystemTimeOfDayInformation,
                out timeOfDay,
                Marshal.SizeOf(typeof(SystemTimeOfDayInformation)),
                out retLength
                );

            if (status >= NtStatus.Error)
                Win32.ThrowLastError(status);

            return timeOfDay;
        }

        public static TimeSpan GetUptime()
        {
            var timeOfDay = GetTimeOfDay();

            return new TimeSpan(timeOfDay.CurrentTime - timeOfDay.BootTime);
        }

        public static void LoadDriver(string serviceName)
        {
            var str = new UnicodeString(
                "\\REGISTRY\\MACHINE\\SYSTEM\\CurrentControlSet\\Services\\" + serviceName);

            try
            {
                NtStatus status;

                if ((status = Win32.NtLoadDriver(ref str)) >= NtStatus.Error)
                    Win32.ThrowLastError(status);
            }
            finally
            {
                str.Dispose();
            }
        }

        private static LargeInteger QueryKSystemTime(IntPtr time)
        {
            unsafe
            {
                return QueryKSystemTime((KSystemTime*)time.ToPointer());
            }
        }

        private unsafe static LargeInteger QueryKSystemTime(KSystemTime* time)
        {
            LargeInteger localTime = new LargeInteger();

            if (IntPtr.Size == 4)
            {
                localTime.QuadPart = 0;

                while (true)
                {
                    localTime.HighPart = time->High1Time;
                    localTime.LowPart = time->LowPart;

                    if (localTime.HighPart == time->High2Time)
                        break;

                    System.Threading.Thread.SpinWait(1);
                }
            }
            else
            {
                localTime.QuadPart = time->QuadPart;
            }

            return localTime;
        }

        public static void UnloadDriver(string serviceName)
        {
            var str = new UnicodeString(
                "\\REGISTRY\\MACHINE\\SYSTEM\\CurrentControlSet\\Services\\" + serviceName);

            try
            {
                NtStatus status;

                if ((status = Win32.NtUnloadDriver(ref str)) >= NtStatus.Error)
                    Win32.ThrowLastError(status);
            }
            finally
            {
                str.Dispose();
            }
        }
    }

    public enum NetworkProtocol
    {
        Tcp, Udp
    }

    public struct ObjectInformation
    {
        public string OrigName;
        public string BestName;
        public string TypeName;
    }

    public struct NetworkConnection
    {
        public int Pid;
        public NetworkProtocol Protocol;
        public IPEndPoint Local;
        public IPEndPoint Remote;
        public MibTcpState State;
        public object Tag;

        public void CloseTcpConnection()
        {
            MibTcpRow row = new MibTcpRow()
            {
                State = MibTcpState.DeleteTcb,
                LocalAddress = (uint)this.Local.Address.Address,
                LocalPort = ((ushort)this.Local.Port).SwapBytes(),
                RemoteAddress = this.Remote != null ? (uint)this.Remote.Address.Address : 0,
                RemotePort = this.Remote != null ? ((ushort)this.Remote.Port).SwapBytes() : 0
            };
            int result = Win32.SetTcpEntry(ref row);

            if (result != 0)
                Win32.ThrowLastError(result);
        }
    }

    public struct SystemProcess
    {
        public string Name;
        public SystemProcessInformation Process;
        public Dictionary<int, SystemThreadInformation> Threads;
    }

    public class KernelModule : ILoadedModule
    {
        public KernelModule(
            IntPtr baseAddress,
            int size,
            LdrpDataTableEntryFlags flags,
            string baseName,
            string fileName
            )
        {
            this.BaseAddress = baseAddress;
            this.Size = size;
            this.Flags = flags;
            this.BaseName = baseName;
            this.FileName = fileName;
        }

        /// <summary>
        /// The base address of the module.
        /// </summary>
        public IntPtr BaseAddress { get; private set; }
        /// <summary>
        /// The size of the module.
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// The flags set by the loader for this module.
        /// </summary>
        public LdrpDataTableEntryFlags Flags { get; private set; }
        /// <summary>
        /// The base name of the module (e.g. module.dll).
        /// </summary>
        public string BaseName { get; private set; }
        /// <summary>
        /// The file name of the module (e.g. C:\Windows\system32\module.dll).
        /// </summary>
        public string FileName { get; private set; }
    }

    public class SystemPagefile
    {
        public SystemPagefile(int totalSize, int totalInUse, int peakUsage, string fileName)
        {
            this.TotalSize = totalSize;
            this.TotalInUse = TotalInUse;
            this.PeakUsage = peakUsage;
            this.FileName = fileName;
        }

        public int TotalSize { get; private set; }
        public int TotalInUse { get; private set; }
        public int PeakUsage { get; private set; }
        public string FileName { get; private set; }
    }
}
