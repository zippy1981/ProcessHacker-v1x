﻿/*
 * Process Hacker
 * 
 * Copyright (C) 2008 wj32
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Runtime.InteropServices;

namespace ProcessHacker
{
    public partial class Win32
    {
        /// <summary>
        /// Represents a handle to the Windows service manager.
        /// </summary>
        public class ServiceManagerHandle : ServiceBaseHandle
        {
            /// <summary>
            /// Connects to the Windows service manager.
            /// </summary>
            /// <param name="access">The desired access to the service manager.</param>
            public ServiceManagerHandle(SC_MANAGER_RIGHTS access)
            {
                this.Handle = OpenSCManager(0, 0, access);

                if (this.Handle == 0)
                    throw new Exception(GetLastErrorMessage());
            }
        }
    }
}