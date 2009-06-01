﻿/*
 * Process Hacker - 
 *   windows handle
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
using System.Threading;
using ProcessHacker.Native.Api;
using ProcessHacker.Native.Security;

namespace ProcessHacker.Native.Objects
{
    /// <summary>
    /// Represents a generic Windows handle which acts as a kernel handle by default.
    /// </summary>
    public class NativeHandle : IDisposable, ISecurable, ISynchronizable
    {
        public static NtStatus WaitAll(ISynchronizable[] objects)
        {
            return WaitAll(objects, false, long.MinValue, false);
        }

        public static NtStatus WaitAll(ISynchronizable[] objects, long timeout)
        {
            return WaitAll(objects, false, timeout);
        }

        public static NtStatus WaitAll(ISynchronizable[] objects, bool alertable, long timeout)
        {
            return WaitAll(objects, alertable, timeout, true);
        }

        public static NtStatus WaitAll(ISynchronizable[] objects, bool alertable, long timeout, bool relative)
        {
            return WaitForMultipleObjects(objects, WaitType.WaitAll, alertable, timeout, relative);
        }

        public static NtStatus WaitAny(ISynchronizable[] objects)
        {
            return WaitAny(objects, false, long.MinValue, false);
        }

        public static NtStatus WaitAny(ISynchronizable[] objects, long timeout)
        {
            return WaitAny(objects, false, timeout);
        }

        public static NtStatus WaitAny(ISynchronizable[] objects, bool alertable, long timeout)
        {
            return WaitAny(objects, alertable, timeout, true);
        }

        public static NtStatus WaitAny(ISynchronizable[] objects, bool alertable, long timeout, bool relative)
        {
            return WaitForMultipleObjects(objects, WaitType.WaitAny, alertable, timeout, relative);
        }

        private static NtStatus WaitForMultipleObjects(ISynchronizable[] objects, WaitType waitType, bool alertable, long timeout, bool relative)
        {
            NtStatus status;
            IntPtr[] handles = new IntPtr[objects.Length];
            long realTimeout = relative ? -timeout : timeout;

            for (int i = 0; i < objects.Length; i++)
                handles[i] = objects[i].Handle;

            if ((status = Win32.NtWaitForMultipleObjects(
                handles.Length,
                handles,
                waitType,
                alertable,
                ref realTimeout
                )) >= NtStatus.Error)
                Win32.ThrowLastError(status);

            return status;
        }

        public static implicit operator int(NativeHandle handle)
        {
            return handle.Handle.ToInt32();
        }

        public static implicit operator IntPtr(NativeHandle handle)
        {
            return handle.Handle;
        }

        private object _disposeLock = new object();
        private bool _owned = true;
        private bool _disposed = false;
        private IntPtr _handle;

        /// <summary>
        /// Creates a new, invalid handle. You must set the handle using the Handle property.
        /// </summary>
        protected NativeHandle()
        { }

        /// <summary>
        /// Creates a new handle using the specified value. The handle will be closed when 
        /// this object is disposed or garbage-collected.
        /// </summary>
        /// <param name="handle">The handle value.</param>
        public NativeHandle(IntPtr handle)
        {
            _handle = handle;
        }

        /// <summary>
        /// Creates a new handle using the specified value. If owned is set to false, the 
        /// handle will not be closed automatically.
        /// </summary>
        /// <param name="handle">The handle value.</param>
        /// <param name="owned">Specifies whether the handle will be closed automatically.</param>
        public NativeHandle(IntPtr handle, bool owned)
        {
            _handle = handle;
            _owned = owned;
        }

        ~NativeHandle()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Closes the handle.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                    Monitor.Enter(_disposeLock);

                if (!_disposed && _owned)
                {
                    this.Close();
                    _disposed = true;
                }
            }
            finally
            {
                if (disposing)
                    Monitor.Exit(_disposeLock);
            }
        }

        /// <summary>
        /// Closes the handle. This method must not be called directly; instead, 
        /// override this method in a derived class if your handle must be closed 
        /// with a method other than CloseHandle.
        /// </summary>
        protected virtual void Close()
        {
            Win32.CloseHandle(_handle);
        }

        /// <summary>
        /// Gets whether this handle is closed.
        /// </summary>
        public bool Disposed
        {
            get { return _disposed; }
        }

        /// <summary>
        /// Gets whether the handle will be automatically closed.
        /// </summary>
        public bool Owned
        {
            get { return _owned; }
        }

        /// <summary>
        /// Gets the handle value.
        /// </summary>
        public IntPtr Handle
        {
            get { return _handle; }
            protected set { _handle = value; }
        }

        /// <summary>
        /// Gets certain information about the handle.
        /// </summary>
        /// <returns>A HANDLE_FLAGS value.</returns>
        public Win32HandleFlags GetHandleFlags()
        {
            Win32HandleFlags flags;

            if (!Win32.GetHandleInformation(this, out flags))
                Win32.ThrowLastError();

            return flags;
        }

        /// <summary>
        /// Gets the handle's name.
        /// </summary>
        /// <returns>A string.</returns>
        public string GetObjectName()
        {
            NtStatus status;
            int retLength;

            status = Win32.NtQueryObject(this, ObjectInformationClass.ObjectNameInformation,
                  IntPtr.Zero, 0, out retLength);

            if (retLength > 0)
            {
                using (MemoryAlloc oniMem = new MemoryAlloc(retLength))
                {
                    if ((status = Win32.NtQueryObject(this, ObjectInformationClass.ObjectNameInformation,
                        oniMem, oniMem.Size, out retLength)) >= NtStatus.Error)
                        Win32.ThrowLastError(status);

                    var oni = oniMem.ReadStruct<ObjectNameInformation>();

                    return oni.Name.Read();
                }
            }
            else
            {
                Win32.ThrowLastError(status);
            }

            return null;
        }

        /// <summary>
        /// Gets the handle's type name.
        /// </summary>
        /// <returns>A string.</returns>
        public string GetObjectTypeName()
        {
            NtStatus status;
            int retLength;

            status = Win32.NtQueryObject(this, ObjectInformationClass.ObjectTypeInformation,
                  IntPtr.Zero, 0, out retLength);

            if (retLength > 0)
            {
                using (MemoryAlloc otiMem = new MemoryAlloc(retLength))
                {
                    if ((status = Win32.NtQueryObject(this, ObjectInformationClass.ObjectTypeInformation,
                        otiMem, otiMem.Size, out retLength)) >= NtStatus.Error)
                        Win32.ThrowLastError(status);

                    var oni = otiMem.ReadStruct<ObjectTypeInformation>();

                    return oni.Name.Read();
                }
            }
            else
            {
                Win32.ThrowLastError(status);
            }

            return null;
        }

        public virtual SecurityDescriptor GetSecurity()
        {
            return this.GetSecurity(SeObjectType.KernelObject);
        }

        protected SecurityDescriptor GetSecurity(SeObjectType objectType)
        {
            int result;
            IntPtr dummy, securityDescriptor;

            if ((result = Win32.GetSecurityInfo(
                this,
                objectType,
                0,
                out dummy, out dummy, out dummy, out dummy,
                out securityDescriptor
                )) != 0)
                Win32.ThrowLastError(result);

            return new SecurityDescriptor(LocalMemoryAlloc.FromPointer(securityDescriptor));
        }

        /// <summary>
        /// Makes the object referenced by the handle permanent.
        /// </summary>
        public void MakeObjectPermanent()
        {
            NtStatus status;

            if ((status = Win32.NtMakePermanentObject(this)) >= NtStatus.Error)
                Win32.ThrowLastError(status);
        }

        /// <summary>
        /// Makes the object referenced by the handle temporary. The object 
        /// will be deleted once the last handle to it is closed.
        /// </summary>
        public void MakeObjectTemporary()
        {
            NtStatus status;

            if ((status = Win32.NtMakeTemporaryObject(this)) >= NtStatus.Error)
                Win32.ThrowLastError(status);
        }

        /// <summary>
        /// Sets certain information about the handle.
        /// </summary>
        /// <param name="mask">Specifies which flags to set.</param>
        /// <param name="flags">The values of the flags to set.</param>
        public void SetHandleFlags(Win32HandleFlags mask, Win32HandleFlags flags)
        {
            if (!Win32.SetHandleInformation(this, mask, flags))
                Win32.ThrowLastError();
        }

        public virtual void SetSecurity(SecurityDescriptor securityDescriptor)
        {
            this.SetSecurity(SeObjectType.KernelObject, securityDescriptor);
        }

        public virtual void SetSecurity(SecurityInformation securityInformation, SecurityDescriptor securityDescriptor)
        {
            this.SetSecurity(SeObjectType.KernelObject, securityInformation, securityDescriptor);
        }

        protected void SetSecurity(SeObjectType objectType, SecurityDescriptor securityDescriptor)
        {
            int result;
            IntPtr owner, group, dacl, sacl;
            bool present, dummy;
            SecurityInformation si = SecurityInformation.Group | SecurityInformation.Owner;

            owner = securityDescriptor.GetOwner(out dummy);  
            group = securityDescriptor.GetGroup(out dummy);
            dacl = securityDescriptor.GetDacl(out present, out dummy);
            if (present)
                si |= SecurityInformation.Dacl;
            sacl = securityDescriptor.GetSacl(out present, out dummy);
            if (present)
                si |= SecurityInformation.Sacl;

            if ((result = Win32.SetSecurityInfo(
                this,
                objectType,
                si,
                owner,
                group,
                dacl,
                sacl
                )) != 0)
                Win32.ThrowLastError(result);
        }

        protected void SetSecurity(SeObjectType objectType, SecurityInformation securityInformation, SecurityDescriptor securityDescriptor)
        {
            int result;
            IntPtr owner, group, dacl, sacl;
            bool dummy;

            owner = securityDescriptor.GetOwner(out dummy);
            group = securityDescriptor.GetGroup(out dummy);
            dacl = securityDescriptor.GetDacl(out dummy, out dummy);
            sacl = securityDescriptor.GetSacl(out dummy, out dummy);

            if ((result = Win32.SetSecurityInfo(
                this,
                objectType,
                securityInformation,
                owner,
                group,
                dacl,
                sacl
                )) != 0)
                Win32.ThrowLastError(result);
        }

        /// <summary>
        /// Signals the object and waits for another.
        /// </summary>
        public virtual NtStatus SignalAndWait(ISynchronizable waitObject)
        {
            return this.SignalAndWait(waitObject, false);
        }

        /// <summary>
        /// Signals the object and waits for another.
        /// </summary>
        public virtual NtStatus SignalAndWait(ISynchronizable waitObject, bool alertable)
        {
            return this.SignalAndWait(waitObject, alertable, long.MinValue, false);
        }

        /// <summary>
        /// Signals the object and waits for another.
        /// </summary>
        public virtual NtStatus SignalAndWait(ISynchronizable waitObject, bool alertable, long timeout)
        {
            return this.SignalAndWait(waitObject, alertable, timeout, true);
        }

        /// <summary>
        /// Signals the object and waits for another.
        /// </summary>
        public virtual NtStatus SignalAndWait(ISynchronizable waitObject, bool alertable, long timeout, bool relative)
        {
            NtStatus status;
            long realTimeout = relative ? -timeout : timeout;

            if ((status = Win32.NtSignalAndWaitForSingleObject(
                this,
                waitObject.Handle,
                alertable,
                ref timeout
                )) >= NtStatus.Error)
                Win32.ThrowLastError(status);

            return status;
        }

        /// <summary>
        /// Waits for the object to be signaled.
        /// </summary>
        public virtual NtStatus Wait()
        {
            return this.Wait(false);
        }

        /// <summary>
        /// Waits for the object to be signaled.
        /// </summary>
        /// <param name="alertable">
        /// Whether user-mode APCs can be delivered during the wait.
        /// </param>
        public virtual NtStatus Wait(bool alertable)
        {
            return this.Wait(alertable, long.MinValue, false);
        }

        /// <summary>
        /// Waits for the object to be signaled.
        /// </summary>
        /// <param name="timeout">The timeout, in 100ns units.</param>
        public NtStatus Wait(long timeout)
        {
            return this.Wait(false, timeout);
        }

        /// <summary>
        /// Waits for the object to be signaled.
        /// </summary>
        /// <param name="alertable">
        /// Whether user-mode APCs can be delivered during the wait.
        /// </param>
        /// <param name="timeout">The timeout, in 100ns units.</param>
        public virtual NtStatus Wait(bool alertable, long timeout)
        {
            return this.Wait(alertable, timeout, true);
        }

        /// <summary>
        /// Waits for the object to be signaled.
        /// </summary>
        /// <param name="timeout">The timeout, in 100ns units.</param>
        /// <param name="relative">Whether the timeout value is relative.</param>
        public NtStatus Wait(long timeout, bool relative)
        {
            return this.Wait(false, timeout, relative);
        }

        /// <summary>
        /// Waits for the object to be signaled.
        /// </summary>
        /// <param name="alertable">
        /// Whether user-mode APCs can be delivered during the wait.
        /// </param>
        /// <param name="timeout">The timeout, in 100ns units.</param>
        /// <param name="relative">Whether the timeout value is relative.</param>
        public virtual NtStatus Wait(bool alertable, long timeout, bool relative)
        {
            NtStatus status;
            long realTimeout = relative ? -timeout : timeout;

            if ((status = Win32.NtWaitForSingleObject(
                this,
                alertable,
                ref realTimeout
                )) >= NtStatus.Error)
                Win32.ThrowLastError(status);

            return status;
        }
    }

    /// <summary>
    /// Represents a generic Windows handle which acts as a kernel handle by default.
    /// </summary>
    public class NativeHandle<TAccess> : NativeHandle
        where TAccess : struct
    {
        /// <summary>
        /// Creates a new, invalid handle. You must set the handle using the Handle property.
        /// </summary>
        protected NativeHandle()
        { }

        /// <summary>
        /// Creates a new handle using the specified value. The handle will be closed when 
        /// this object is disposed or garbage-collected.
        /// </summary>
        /// <param name="handle">The handle value.</param>
        public NativeHandle(IntPtr handle)
            : base(handle)
        { }

        /// <summary>
        /// Creates a new handle using the specified value. If owned is set to false, the 
        /// handle will not be closed automatically.
        /// </summary>
        /// <param name="handle">The handle value.</param>
        /// <param name="owned">Specifies whether the handle will be closed automatically.</param>
        public NativeHandle(IntPtr handle, bool owned)
            : base(handle, owned)
        { }

        /// <summary>
        /// Creates a new handle by duplicating an existing handle.
        /// </summary>
        /// <param name="handle">The existing handle.</param>
        /// <param name="desiredAccess">The desired access to the object.</param>
        public NativeHandle(IntPtr handle, TAccess access)
        {
            IntPtr newHandle;

            Win32.DuplicateObject(ProcessHandle.GetCurrent(), handle, ProcessHandle.GetCurrent(), out newHandle,
                (int)Convert.ChangeType(access, typeof(int)), 0, 0);
            this.Handle = newHandle;
        }

        /// <summary>
        /// Creates a new handle by duplicating an existing handle from another process.
        /// </summary>
        /// <param name="processHandle">A handle to a process. It must have the PROCESS_DUP_HANDLE permission.</param>
        /// <param name="handle">The existing handle.</param>
        /// <param name="desiredAccess">The desired access to the object.</param>
        public NativeHandle(ProcessHandle processHandle, IntPtr handle, TAccess access)
        {
            IntPtr newHandle;

            Win32.DuplicateObject(processHandle, handle, ProcessHandle.GetCurrent(), out newHandle,
                (int)Convert.ChangeType(access, typeof(int)), 0, 0);
            this.Handle = newHandle;
        }

        /// <summary>
        /// Duplicates the handle.
        /// </summary>
        /// <param name="desiredAccess">The desired access to the object.</param>
        /// <returns>A handle.</returns>
        public NativeHandle<TAccess> Duplicate(TAccess access)
        {
            return new NativeHandle<TAccess>(ProcessHandle.GetCurrent(), this, access);
        }
    }

    /// <summary>
    /// Represents a generic Windows handle which acts as a kernel handle by default.
    /// </summary>
    public class GenericHandle : NativeHandle<int>
    {
        /// <summary>
        /// Creates a new, invalid handle. You must set the handle using the Handle property.
        /// </summary>
        protected GenericHandle()
            : base()
        { }

        /// <summary>
        /// Creates a new handle using the specified value. The handle will be closed when 
        /// this object is disposed or garbage-collected.
        /// </summary>
        /// <param name="handle">The handle value.</param>
        public GenericHandle(IntPtr handle)
            : base(handle)
        { }

        /// <summary>
        /// Creates a new handle using the specified value. If owned is set to false, the 
        /// handle will not be closed automatically.
        /// </summary>
        /// <param name="handle">The handle value.</param>
        /// <param name="owned">Specifies whether the handle will be closed automatically.</param>
        public GenericHandle(IntPtr handle, bool owned)
            : base(handle, owned)
        { }

        /// <summary>
        /// Creates a new handle by duplicating an existing handle.
        /// </summary>
        /// <param name="handle">The existing handle.</param>
        /// <param name="desiredAccess">The desired access to the object.</param>
        public GenericHandle(IntPtr handle, int desiredAccess)
            : base(handle, desiredAccess)
        { }

        /// <summary>
        /// Creates a new handle by duplicating an existing handle from another process.
        /// </summary>
        /// <param name="processHandle">A handle to a process. It must have the PROCESS_DUP_HANDLE permission.</param>
        /// <param name="handle">The existing handle.</param>
        /// <param name="desiredAccess">The desired access to the object.</param>
        public GenericHandle(ProcessHandle processHandle, IntPtr handle, int desiredAccess)
            : base(processHandle, handle, desiredAccess)
        { }
    }
}