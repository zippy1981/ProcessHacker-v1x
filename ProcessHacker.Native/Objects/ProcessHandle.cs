﻿/*
 * Process Hacker - 
 *   process handle
 * 
 * Copyright (C) 2008-2009 wj32
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using ProcessHacker.Native.Api;
using ProcessHacker.Native.Security;

namespace ProcessHacker.Native.Objects
{
    /// <summary>
    /// Represents a handle to a Windows process.
    /// </summary>
    /// <remarks>The idea of a ProcessHandle class is 
    /// different to the <see cref="System.Diagnostics.Process"/> class; 
    /// instead of opening the process with the right permissions every 
    /// time a query or set function is called, this lets the users control 
    /// when they want to open handles with certain permissions. This 
    /// means that handles can be cached (by the users).</remarks>
    public class ProcessHandle : Win32Handle<ProcessAccess>, IWithToken
    {
        /// <summary>
        /// Creates a process handle using an existing handle. 
        /// The handle will not be closed automatically.
        /// </summary>
        /// <param name="Handle">The handle value.</param>
        /// <returns>The process handle.</returns>
        public static ProcessHandle FromHandle(int handle)
        {
            return new ProcessHandle(handle, false);
        }

        internal ProcessHandle(int handle, bool owned)
            : base(handle, owned)
        { }

        /// <summary>
        /// Creates a new process handle.
        /// </summary>
        /// <param name="PID">The ID of the process to open.</param>
        public ProcessHandle(int pid)
            : this(pid, ProcessAccess.All)
        { }

        /// <summary>
        /// Creates a new process handle.
        /// </summary>
        /// <param name="PID">The ID of the process to open.</param>
        /// <param name="access">The desired access to the process.</param>
        public ProcessHandle(int pid, ProcessAccess access)
        {
            if (KProcessHacker.Instance != null)
                this.Handle = KProcessHacker.Instance.KphOpenProcess(pid, access);
            else
                this.Handle = Win32.OpenProcess(access, 0, pid);

            if (this.Handle == 0)
                Win32.ThrowLastWin32Error();
        }

        /// <summary>
        /// Allocates a memory region in the process' virtual memory.
        /// </summary>      
        /// <param name="address">The base address of the region.</param>
        /// <param name="size">The size of the region.</param>
        /// <param name="protection">The protection of the region.</param>
        /// <returns>The base address of the allocated pages.</returns>
        public int AllocMemory(int address, int size, MemoryProtection protection)
        {
            int newAddress;

            if ((newAddress = Win32.VirtualAllocEx(this, address, size, MemoryState.Commit, protection))
                == 0)
                Win32.ThrowLastWin32Error();

            return newAddress;
        }

        /// <summary>
        /// Allocates a memory region in the process' virtual memory. The function decides where 
        /// to allocate the memory.
        /// </summary>
        /// <param name="size">The size of the region.</param>
        /// <param name="protection">The protection of the region.</param>
        /// <returns>The base address of the allocated pages.</returns>
        public int AllocMemory(int size, MemoryProtection protection)
        {
            return this.AllocMemory(0, size, protection);
        }

        /// <summary>
        /// Assigns the process to a job object. The job handle must have the 
        /// JOB_OBJECT_ASSIGN_PROCESS permission and the process handle must have 
        /// the PROCESS_SET_QUOTA and PROCESS_TERMINATE permissions.
        /// </summary>
        /// <param name="job">The job object to assign the process to.</param>
        public void AssignToJobObject(JobObjectHandle job)
        {
            if (!Win32.AssignProcessToJobObject(job, this))
                Win32.ThrowLastWin32Error();
        }

        /// <summary>
        /// Creates a remote thread in the process.
        /// </summary>
        /// <param name="startAddress">The address at which to begin execution (e.g. a function). The 
        /// function must be accessible from the remote process; that is, it must be in its 
        /// virtual address space, either copied using AllocMemory or loaded as module using 
        /// LoadLibrary.
        /// </param>
        /// <param name="parameter">The parameter to pass to the function.</param>
        /// <returns>The ID of the new thread.</returns>
        public int CreateThread(int startAddress, int parameter)
        {
            int threadId;

            if (!Win32.CreateRemoteThread(this, 0, 0, startAddress, parameter, 0, out threadId))
                Win32.ThrowLastWin32Error();

            return threadId;
        }

        /// <summary>
        /// Creates a remote thread in the process, returning a handle to the new thread.
        /// </summary>
        /// <param name="startAddress">The address at which to begin execution (e.g. a function). The 
        /// function must be accessible from the remote process; that is, it must be in its 
        /// virtual address space, either copied using AllocMemory or loaded as module using 
        /// LoadLibrary.
        /// </param>
        /// <param name="parameter">The parameter to pass to the function.</param>
        /// <param name="access">The desired access to the new thread.</param>
        /// <returns>A handle to the new thread.</returns>
        public ThreadHandle CreateThread(int startAddress, int parameter, ThreadAccess access)
        {
            return new ThreadHandle(this.CreateThread(startAddress, parameter), access);
        }

        /// <summary>
        /// Removes as many pages as possible from the process' working set. This requires the 
        /// PROCESS_QUERY_INFORMATION and PROCESS_SET_INFORMATION permissions.
        /// </summary>
        public void EmptyWorkingSet()
        {
            if (!Win32.EmptyWorkingSet(this))
                Win32.ThrowLastWin32Error();
        }

        /// <summary>
        /// Frees a memory region in the process' virtual memory.
        /// </summary>
        /// <param name="address">The address of the region to free.</param>
        /// <param name="size">The size to free.</param>
        /// <param name="reserveOnly">Specifies whether or not to only 
        /// reserve the memory instead of freeing it.</param>
        public void FreeMemory(int address, int size, bool reserveOnly)
        {
            // size needs to be 0 if we're freeing
            if (!reserveOnly)
                size = 0;

            if (!Win32.VirtualFreeEx(this, address, size,
                reserveOnly ? MemoryState.Decommit : MemoryState.Release))
                Win32.ThrowLastWin32Error();
        }

        /// <summary>
        /// Gets the process' basic information through the undocumented Native API function 
        /// NtQueryInformationProcess. This function requires the PROCESS_QUERY_LIMITED_INFORMATION 
        /// permission.
        /// </summary>
        /// <returns>A PROCESS_BASIC_INFORMATION structure.</returns>
        public ProcessBasicInformation GetBasicInformation()
        {
            ProcessBasicInformation pbi = new ProcessBasicInformation();
            int retLen;

            if (Win32.NtQueryInformationProcess(this, ProcessInformationClass.ProcessBasicInformation,
                ref pbi, Marshal.SizeOf(pbi), out retLen) < 0)
                Win32.ThrowLastWin32Error();

            return pbi;
        }

        /// <summary>
        /// Gets the command line used to start the process. This requires 
        /// the PROCESS_QUERY_LIMITED_INFORMATION and PROCESS_VM_READ permissions.
        /// </summary>
        /// <returns>A string.</returns>
        public string GetCommandLine()
        {
            return this.GetPebString(PebOffset.CommandLine);
        }

        /// <summary>
        /// Gets the number of processor cycles consumed by the process' threads.
        /// </summary>
        public ulong GetCycleTime()
        {
            ulong cycles;

            if (!Win32.QueryProcessCycleTime(this, out cycles))
                Win32.ThrowLastWin32Error();

            return cycles;
        }

        /// <summary>
        /// Gets the process' DEP policy.
        /// </summary>
        /// <returns>A DEPStatus enum.</returns>
        /// <remarks>This function does not work on 
        /// Windows XP SP2 or lower, since they do not 
        /// have the GetProcessDEPPolicy function. It is possible 
        /// to use NtQueryInformationProcess with ProcessExecuteFlags, 
        /// but it doesn't seem to work.</remarks>
        public DepStatus GetDepStatus()
        {
            DepFlags flags;
            int perm;

            if (!Win32.GetProcessDEPPolicy(this, out flags, out perm))
                Win32.ThrowLastWin32Error();

            return
                ((flags & DepFlags.Enable) != 0 ? DepStatus.Enabled : 0) |
                ((flags & DepFlags.DisableAtlThunkEmulation) != 0 ?
                (DepStatus.Enabled | DepStatus.AtlThunkEmulationDisabled) : 0) |
                ((perm != 0) ? DepStatus.Permanent : 0);
        }

        /// <summary>
        /// Gets the process' environment variables. This requires the 
        /// PROCESS_QUERY_INFORMATION and PROCESS_VM_READ permissions.
        /// </summary>
        /// <returns>A dictionary of variables.</returns>
        public unsafe IDictionary<string, string> GetEnvironmentVariables()
        {
            int pebBaseAddress = this.GetBasicInformation().PebBaseAddress;
            byte* buffer = stackalloc byte[4];

            this.ReadMemory(pebBaseAddress + 0x10, buffer, 4);
            int processParameters = *(int*)buffer;

            /*
             * RTL_USER_PROCESS_PARAMETERS
             * off field
             * +00 ULONG MaximumLength
             * +04 ULONG Length
             * +08 ULONG Flags
             * +0c ULONG DebugFlags
             * +10 PVOID ConsoleHandle
             * +14 ULONG ConsoleFlags
             * +18 HANDLE StdInputHandle
             * +1c HANDLE StdOutputHandle
             * +20 HANDLE StdErrorHandle
             * +24 UNICODE_STRING CurrentDirectoryPath
             * +2c HANDLE CurrentDirectoryHandle
             * +30 UNICODE_STRING DllPath
             * +38 UNICODE_STRING ImagePathName
             * +40 UNICODE_STRING CommandLine
             * +48 PVOID Environment
             */
            this.ReadMemory(processParameters + 0x48, buffer, 4);
            int envBase = *(int*)buffer;
            int length = 0;

            {
                MemoryBasicInformation mbi = new MemoryBasicInformation();

                if (!Win32.VirtualQueryEx(this, envBase, ref mbi, Marshal.SizeOf(mbi)))
                    Win32.ThrowLastWin32Error();
                if (mbi.Protect == MemoryProtection.NoAccess)
                    throw new WindowsException();

                length = mbi.RegionSize - (envBase - mbi.BaseAddress);
            }

            // Now we read in the entire region of memory
            // And yes, some memory is wasted.
            byte[] memory = this.ReadMemory(envBase, length);

            /* The environment variables block is a series of Unicode strings separated by 
             * two null bytes. The entire block is terminated by four null bytes.
             */
            Dictionary<string, string> vars = new Dictionary<string, string>();
            StringBuilder currentVariable = new StringBuilder();
            int i = 0;

            while (true)
            {
                char currentChar =
                    UnicodeEncoding.Unicode.GetChars(memory, i, 2)[0];

                i += 2;

                if (currentChar == '\0')
                {
                    // Two nulls in a row, the env. block is finished.
                    if (currentVariable.Length == 0)
                        break;

                    string[] s = currentVariable.ToString().Split(new char[] { '=' }, 2);

                    if (!vars.ContainsKey(s[0]) && s.Length > 1)
                        vars.Add(s[0], s[1]);

                    currentVariable = new StringBuilder();
                }
                else
                {
                    currentVariable.Append(currentChar);
                }
            }

            return vars;
        }

        /// <summary>
        /// Gets the process' exit code.
        /// </summary>
        /// <returns>A number.</returns>
        public int GetExitCode()
        {
            int exitCode;

            if (!Win32.GetExitCodeProcess(this, out exitCode))
                Win32.ThrowLastWin32Error();

            return exitCode;
        }

        /// <summary>
        /// Gets a GUI handle count.
        /// </summary>
        /// <param name="userObjects">If true, returns the number of USER handles. Otherwise, returns 
        /// the number of GDI handles.</param>
        /// <returns>A handle count.</returns>
        public int GetGuiResources(bool userObjects)
        {
            return Win32.GetGuiResources(this, userObjects);
        }

        /// <summary>
        /// Gets the file name of the process' image. This requires the
        /// PROCESS_QUERY_LIMITED_INFORMATION permission.
        /// </summary>
        /// <returns>A file name, in DOS (normal) format.</returns>
        public string GetImageFileName()
        {
            var sb = new StringBuilder(1024);
            int len = 1024;

            if (!Win32.QueryFullProcessImageName(this, false, sb, ref len))
                Win32.ThrowLastWin32Error();

            return FileUtils.FixPath(sb.ToString(0, len));
        }

        /// <summary>
        /// Opens the job associated with the process.
        /// </summary>
        /// <returns>A job handle.</returns>
        public JobObjectHandle GetJob(JobObjectAccess access)
        {
            return new JobObjectHandle(this, access);
        }

        /// <summary>
        /// Gets the main module of the process. This requires the 
        /// PROCESS_QUERY_INFORMATION and PROCESS_VM_READ permissions.
        /// </summary>
        /// <returns>A ProcessModule.</returns>
        public ProcessModule GetMainModule()
        {
            return this.GetMainModuleNative();
        }

        private ProcessModule GetMainModuleApi()
        {
            IntPtr[] moduleHandles;
            int requiredSize;

            Win32.EnumProcessModules(this, null, 0, out requiredSize);
            moduleHandles = new IntPtr[requiredSize / 4];

            if (!Win32.EnumProcessModules(this, moduleHandles, requiredSize, out requiredSize))
                Win32.ThrowLastWin32Error();

            ModuleInfo moduleInfo = new ModuleInfo();
            StringBuilder baseName = new StringBuilder(0x400);
            StringBuilder fileName = new StringBuilder(0x400);

            if (!Win32.GetModuleInformation(this, moduleHandles[0], ref moduleInfo, Marshal.SizeOf(moduleInfo)))
                Win32.ThrowLastWin32Error();
            if (Win32.GetModuleBaseName(this, moduleHandles[0], baseName, baseName.Capacity * 2) == 0)
                Win32.ThrowLastWin32Error();
            if (Win32.GetModuleFileNameEx(this, moduleHandles[0], fileName, fileName.Capacity * 2) == 0)
                Win32.ThrowLastWin32Error();

            return new ProcessModule(
                moduleInfo.BaseOfDll, moduleInfo.SizeOfImage, moduleInfo.EntryPoint,
                baseName.ToString(), FileUtils.FixPath(fileName.ToString())
                );
        }

        private unsafe ProcessModule GetMainModuleNative()
        {
            byte* buffer = stackalloc byte[4];

            this.ReadMemory(this.GetBasicInformation().PebBaseAddress + 0xc, buffer, 4);

            int loaderData = *(int*)buffer;

            PebLdrData* data = stackalloc PebLdrData[1];
            this.ReadMemory(loaderData, data, Marshal.SizeOf(typeof(PebLdrData)));

            if (data->Initialized == 0)
                throw new Exception("Loader data is not initialized.");

            LdrModule* mainModule = stackalloc LdrModule[1];

            this.ReadMemory(data->InLoadOrderModuleList.Flink, mainModule, Marshal.SizeOf(typeof(LdrModule)));

            return new ProcessModule(
                new IntPtr(mainModule->BaseAddress),
                mainModule->SizeOfImage,
                new IntPtr(mainModule->EntryPoint),
                Utils.ReadUnicodeString(this, mainModule->BaseDllName).TrimEnd('\0'),
                FileUtils.FixPath(Utils.ReadUnicodeString(this, mainModule->FullDllName).TrimEnd('\0'))
                );
        }

        /// <summary>
        /// Gets the modules loaded by the process. This requires the 
        /// PROCESS_QUERY_INFORMATION and PROCESS_VM_READ permissions.
        /// </summary>
        /// <returns>An array of ProcessModule objects.</returns>
        public ProcessModule[] GetModules()
        {
            return this.GetModulesNative();
        }

        private ProcessModule[] GetModulesApi()
        {
            IntPtr[] moduleHandles;
            int requiredSize;

            Win32.EnumProcessModules(this, null, 0, out requiredSize);
            moduleHandles = new IntPtr[requiredSize / 4];

            if (!Win32.EnumProcessModules(this, moduleHandles, requiredSize, out requiredSize))
                Win32.ThrowLastWin32Error();

            ProcessModule[] moduleList = new ProcessModule[moduleHandles.Length];

            for (int i = 0; i < moduleHandles.Length; i++)
            {
                ModuleInfo moduleInfo = new ModuleInfo();
                StringBuilder baseName = new StringBuilder(0x400);
                StringBuilder fileName = new StringBuilder(0x400);

                if (!Win32.GetModuleInformation(this, moduleHandles[i], ref moduleInfo, Marshal.SizeOf(moduleInfo)))
                    Win32.ThrowLastWin32Error();
                if (Win32.GetModuleBaseName(this, moduleHandles[i], baseName, baseName.Capacity * 2) == 0)
                    Win32.ThrowLastWin32Error();
                if (Win32.GetModuleFileNameEx(this, moduleHandles[i], fileName, fileName.Capacity * 2) == 0)
                    Win32.ThrowLastWin32Error();

                moduleList[i] = new ProcessModule(
                    moduleInfo.BaseOfDll, moduleInfo.SizeOfImage, moduleInfo.EntryPoint,
                    baseName.ToString(), FileUtils.FixPath(fileName.ToString())
                    );
            }

            return moduleList;
        }

        private unsafe ProcessModule[] GetModulesNative()
        {
            byte* buffer = stackalloc byte[4];

            this.ReadMemory(this.GetBasicInformation().PebBaseAddress + 0xc, buffer, 4);

            int loaderData = *(int*)buffer;

            PebLdrData* data = stackalloc PebLdrData[1];
            this.ReadMemory(loaderData, data, Marshal.SizeOf(typeof(PebLdrData)));

            if (data->Initialized == 0)
                throw new Exception("Loader data is not initialized.");

            List<ProcessModule> modules = new List<ProcessModule>();
            IntPtr currentLink = data->InLoadOrderModuleList.Flink;
            IntPtr startLink = currentLink;
            LdrModule* currentModule = stackalloc LdrModule[1];
            int i = 0;

            while (currentLink != IntPtr.Zero)
            {
                // Stop when we have reached the beginning of the linked list
                if (modules.Count > 0 && currentLink == startLink)
                    break;
                // Safety guard
                if (i > 0x800)
                    break;

                this.ReadMemory(currentLink, currentModule, Marshal.SizeOf(typeof(LdrModule)));

                if (currentModule->BaseAddress != 0)
                {
                    modules.Add(new ProcessModule(
                        new IntPtr(currentModule->BaseAddress),
                        currentModule->SizeOfImage,
                        new IntPtr(currentModule->EntryPoint),
                        Utils.ReadUnicodeString(this, currentModule->BaseDllName).TrimEnd('\0'),
                        FileUtils.FixPath(Utils.ReadUnicodeString(this, currentModule->FullDllName).TrimEnd('\0'))
                        ));
                }

                currentLink = currentModule->InLoadOrderModuleList.Flink;
                i++;
            }

            return modules.ToArray();
        }

        /// <summary>
        /// Gets the file name of the process' image, in device name format. This 
        /// requires the PROCESS_QUERY_LIMITED_INFORMATION permission.
        /// </summary>
        /// <returns>A file name, in device/native format.</returns>
        public string GetNativeImageFileName()
        {
            int retLen;

            Win32.NtQueryInformationProcess(this, ProcessInformationClass.ProcessImageFileName,
                IntPtr.Zero, 0, out retLen);

            using (MemoryAlloc data = new MemoryAlloc(retLen))
            {
                if (Win32.NtQueryInformationProcess(this, ProcessInformationClass.ProcessImageFileName,
                    data, retLen, out retLen) < 0)
                    Win32.ThrowLastWin32Error();

                UnicodeString str = data.ReadStruct<UnicodeString>();

                return Utils.ReadUnicodeString(str);
            }
        }

        /// <summary>
        /// Gets the process' parent's process ID. This requires 
        /// the PROCESS_QUERY_LIMITED_INFORMATION permission.
        /// </summary>
        /// <returns>The process ID.</returns>
        public int GetParentPID()
        {
            return this.GetBasicInformation().InheritedFromUniqueProcessId;
        }

        /// <summary>
        /// Reads a UNICODE_STRING from the process' process environment block.
        /// </summary>
        /// <param name="offset">The offset to the UNICODE_STRING structure.</param>
        /// <returns>A string.</returns>
        public unsafe string GetPebString(PebOffset offset)
        {
            byte* buffer = stackalloc byte[4];
            int pebBaseAddress = this.GetBasicInformation().PebBaseAddress;

            /* read address of parameter information block
             *
             * PEB
             * off field
             * +00 BOOLEAN InheritedAddressSpace;
             * +01 BOOLEAN ReadImageFileExecOptions;
             * +02 BOOLEAN BeingDebugged;
             * +03 BOOLEAN Spare;
             * +04 HANDLE Mutant;
             * +08 PVOID ImageBaseAddress;
             * +0c PVOID LoaderData;
             * +10 PRTL_USER_PROCESS_PARAMETERS ProcessParameters; 
             */
            this.ReadMemory(pebBaseAddress + 0x10, buffer, 4);
            int processParameters = *(int*)buffer;

            // Read length (in bytes) of string. The offset of the UNICODE_STRING structure is 
            // specified in the enum.
            //
            // UNICODE_STRING
            // off field
            // +00 USHORT Length;
            // +02 USHORT MaximumLength;
            // +04 PWSTR Buffer;
            this.ReadMemory(processParameters + (int)offset, buffer, 2);
            ushort stringLength = *(ushort*)buffer;
            byte[] stringData = new byte[stringLength];

            // read address of string
            this.ReadMemory(processParameters + (int)offset + 0x4, buffer, 4);
            int stringAddr = *(int*)buffer;

            // read string and decode it
            return UnicodeEncoding.Unicode.GetString(
                this.ReadMemory(stringAddr, stringLength)).TrimEnd('\0');
        }

        /// <summary>
        /// Gets the process' priority class.
        /// </summary>
        /// <returns>A ProcessPriorityClass enum.</returns>
        public ProcessPriorityClass GetPriorityClass()
        {
            int priority = Win32.GetPriorityClass(this);

            if (priority == 0)
                Win32.ThrowLastWin32Error();

            return (ProcessPriorityClass)priority;
        }

        /// <summary>
        /// Forces the process to load the specified library.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        public void InjectDll(string path)
        {
            this.InjectDll(path, 0xffffffff);
        }

        /// <summary>
        /// Forces the process to load the specified library.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <param name="timeout">The timeout, in seconds, for the process to load the library.</param>
        public void InjectDll(string path, uint timeout)
        {
            int stringPage = this.AllocMemory(path.Length * 2 + 2, MemoryProtection.ExecuteReadWrite);

            this.WriteMemory(stringPage, UnicodeEncoding.Unicode.GetBytes(path));

            this.CreateThread(Win32.GetProcAddress(Win32.GetModuleHandle("kernel32.dll"), "LoadLibraryW"),
                stringPage, ThreadAccess.All).Wait(timeout);

            this.FreeMemory(stringPage, path.Length * 2 + 2, false);
        }

        /// <summary>
        /// Gets whether the process is currently being debugged. This requires 
        /// the PROCESS_QUERY_INFORMATION permission.
        /// </summary>
        /// <returns>A boolean value.</returns>
        public bool IsBeingDebugged()
        {
            bool debugged;

            if (!Win32.CheckRemoteDebuggerPresent(this, out debugged))
                Win32.ThrowLastWin32Error();

            return debugged;
        }

        /// <summary>
        /// Determines whether the process is running in a job.
        /// </summary>
        /// <returns>A boolean.</returns>
        /// <remarks>According to this function, almost every single 
        /// process is in a job! This function does not tell us 
        /// the name of the job though.</remarks>
        public bool IsInJob()
        {
            bool result;

            if (!Win32.IsProcessInJob(this, 0, out result))
                Win32.ThrowLastWin32Error();

            return result;
        }

        /// <summary>
        /// Reads data from the process' virtual memory.
        /// </summary>
        /// <param name="offset">The offset at which to begin reading.</param>
        /// <param name="length">The length, in bytes, to read.</param>
        /// <returns>An array of bytes</returns>
        public byte[] ReadMemory(int offset, int length)
        {
            byte[] buffer = new byte[length];

            this.ReadMemory(offset, buffer, length);

            return buffer;
        }

        /// <summary>
        /// Reads data from the process' virtual memory.
        /// </summary>
        /// <param name="offset">The offset at which to begin reading.</param>
        /// <param name="length">The length, in bytes, to read.</param>
        /// <returns>An array of bytes</returns>
        public byte[] ReadMemory(IntPtr offset, int length)
        {
            return this.ReadMemory(offset.ToInt32(), length);
        }

        public unsafe int ReadMemory(int offset, byte[] buffer, int length)
        {
            fixed (byte* bufferPtr = buffer)
                return this.ReadMemory(offset, bufferPtr, length);
        }

        public unsafe int ReadMemory(int offset, void* buffer, int length)
        {
            int readLen;

            if (KProcessHacker.Instance != null && OSVersion.HasMmCopyVirtualMemory)
            {
                KProcessHacker.Instance.KphReadVirtualMemory(this, offset, buffer, length, out readLen);
            }
            else
            {
                if (!Win32.ReadProcessMemory(this, offset, buffer, length, out readLen))
                    Win32.ThrowLastWin32Error();
            }

            return readLen;
        }

        public unsafe int ReadMemory(IntPtr offset, void* buffer, int length)
        {
            return this.ReadMemory(offset.ToInt32(), buffer, length);
        }

        /// <summary>
        /// Resumes the process. This requires the PROCESS_SUSPEND_RESUME permission.
        /// </summary>
        public void Resume()
        {
            if (KProcessHacker.Instance != null && OSVersion.HasPsSuspendResumeProcess)
            {
                KProcessHacker.Instance.KphResumeProcess(this);
            }
            else
            {
                if (Win32.NtResumeProcess(this) < 0)
                    Win32.ThrowLastWin32Error();
            }
        }

        public unsafe void SetModuleReferenceCount(int baseAddress, ushort count)
        {
            byte* buffer = stackalloc byte[4];

            this.ReadMemory(this.GetBasicInformation().PebBaseAddress + 0xc, buffer, 4);

            int loaderData = *(int*)buffer;

            PebLdrData* data = stackalloc PebLdrData[1];
            this.ReadMemory(loaderData, data, Marshal.SizeOf(typeof(PebLdrData)));

            if (data->Initialized == 0)
                throw new Exception("Loader data is not initialized.");

            List<ProcessModule> modules = new List<ProcessModule>();
            IntPtr currentLink = data->InLoadOrderModuleList.Flink;
            IntPtr startLink = currentLink;
            LdrModule* currentModule = stackalloc LdrModule[1];
            int i = 0;

            while (currentLink != IntPtr.Zero)
            {
                if (modules.Count > 0 && currentLink == startLink)
                    break;
                if (i > 0x800)
                    break;

                this.ReadMemory(currentLink, currentModule, Marshal.SizeOf(typeof(LdrModule)));

                if (currentModule->BaseAddress == baseAddress)
                {
                    this.WriteMemory(currentLink.ToInt32() + 0x38, &count, 2);
                    break;
                }

                currentLink = currentModule->InLoadOrderModuleList.Flink;
                i++;
            }
        }

        /// <summary>
        /// Sets the process' priority class.
        /// </summary>
        /// <param name="priority">The process' priority.</param>
        public void SetPriorityClass(ProcessPriorityClass priority)
        {
            if (!Win32.SetPriorityClass(this, (int)priority))
                Win32.ThrowLastWin32Error();
        }

        /// <summary>
        /// Suspends the process. This requires the PROCESS_SUSPEND_RESUME permission.
        /// </summary>
        public void Suspend()
        {
            if (KProcessHacker.Instance != null && OSVersion.HasPsSuspendResumeProcess)
            {
                KProcessHacker.Instance.KphSuspendProcess(this);
            }
            else
            {
                if (Win32.NtSuspendProcess(this) < 0)
                    Win32.ThrowLastWin32Error();
            }
        }

        /// <summary>
        /// Terminates the process. This requires the PROCESS_TERMINATE permission.
        /// </summary>
        public void Terminate()
        {
            this.Terminate(0);
        }

        /// <summary>
        /// Terminates the process, specifying the exit code. This requires the 
        /// PROCESS_TERMINATE permission.
        /// </summary>
        /// <param name="ExitCode">The exit code.</param>
        public void Terminate(int ExitCode)
        {
            if (KProcessHacker.Instance != null)
            {
                KProcessHacker.Instance.KphTerminateProcess(this, ExitCode);
            }
            else
            {
                if (!Win32.TerminateProcess(this, ExitCode))
                    Win32.ThrowLastWin32Error();
            }
        }

        /// <summary>
        /// Writes data to the process' virtual memory.
        /// </summary>
        /// <param name="offset">The offset at which to begin writing.</param>
        /// <param name="data">The data to write.</param>
        /// <returns>The length, in bytes, that was written.</returns>
        public unsafe int WriteMemory(int offset, byte[] data)
        {
            fixed (byte* dataPtr = data)
            {
                return WriteMemory(offset, dataPtr, data.Length);
            }
        }

        /// <summary>
        /// Writes data to the process' virtual memory.
        /// </summary>
        /// <param name="offset">The offset at which to begin writing.</param>
        /// <param name="data">The data to write.</param>
        /// <param name="length">The length to be written.</param>
        /// <returns>The length, in bytes, that was written.</returns>
        public unsafe int WriteMemory(int offset, void* data, int length)
        {
            int writtenLen;

            if (KProcessHacker.Instance != null && OSVersion.HasMmCopyVirtualMemory)
            {
                KProcessHacker.Instance.KphWriteVirtualMemory(this, offset, data, length, out writtenLen);
            }
            else
            {
                if (!Win32.WriteProcessMemory(this, offset, data, length, out writtenLen))
                    Win32.ThrowLastWin32Error();
            }

            return writtenLen;
        }

        /// <summary>
        /// Opens and returns a handle to the process' token. This requires the 
        /// PROCESS_QUERY_LIMITED_INFORMATION permission.
        /// </summary>
        /// <returns>A handle to the process' token.</returns>
        public TokenHandle GetToken()
        {
            return GetToken(TokenAccess.All);
        }

        /// <summary>
        /// Opens and returns a handle to the process' token. This requires the 
        /// PROCESS_QUERY_LIMITED_INFORMATION permission.
        /// </summary>
        /// <param name="access">The desired access to the token.</param>
        /// <returns>A handle to the process' token.</returns>
        public TokenHandle GetToken(TokenAccess access)
        {
            return new TokenHandle(this, access);
        }
    }

    public class ProcessModule
    {
        public ProcessModule(IntPtr baseAddress, int size, IntPtr entryPoint, string baseName, string fileName)
        {
            this.BaseAddress = baseAddress;
            this.Size = size;
            this.EntryPoint = entryPoint;
            this.BaseName = baseName;
            this.FileName = fileName;
        }

        public IntPtr BaseAddress { get; private set; }
        public int Size { get; private set; }
        public IntPtr EntryPoint { get; private set; }
        public string BaseName { get; private set; }
        public string FileName { get; private set; }
    }

    /// <summary>
    /// Specifies the DEP status of a process.
    /// </summary>
    [Flags]
    public enum DepStatus
    {
        /// <summary>
        /// DEP is enabled.
        /// </summary>
        Enabled = 0x1,

        /// <summary>
        /// DEP is permanently enabled or disabled and cannot
        /// be enabled or disabled.
        /// </summary>
        Permanent = 0x2,

        /// <summary>
        /// DEP is enabled with DEP-ATL thunk emulation disabled.
        /// </summary>
        AtlThunkEmulationDisabled = 0x4
    }

    /// <summary>
    /// Specifies an offset in a process' process environment block (PEB).
    /// </summary>
    public enum PebOffset
    {
        /// <summary>
        /// The current directory of the process. This may, as the name 
        /// implies, change very often.
        /// </summary>
        CurrentDirectoryPath = 0x24,

        /// <summary>
        /// A copy of the PATH environment variable for the process.
        /// </summary>
        DllPath = 0x30,

        /// <summary>
        /// The image file name, in kernel format (e.g. \\?\C:\...,
        /// \SystemRoot\..., \Device\Harddisk1\...).
        /// </summary>
        ImagePathName = 0x38,

        /// <summary>
        /// The command used to start the program, including arguments.
        /// </summary>
        CommandLine = 0x40,

        /// <summary>
        /// Usually blank.
        /// </summary>
        WindowTitle = 0x70,

        /// <summary>
        /// For interactive programs, contains the window station and 
        /// desktop name of the first thread that was started, e.g. 
        /// WinSta0\Default.
        /// </summary>
        DesktopName = 0x78,

        /// <summary>
        /// Usually blank.
        /// </summary>
        ShellInfo = 0x80,

        /// <summary>
        /// Usually blank.
        /// </summary>
        RuntimeData = 0x88
    }
}
