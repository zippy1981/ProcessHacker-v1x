﻿/*
 * Process Hacker - 
 *   DEP status editor
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
using System.Windows.Forms;
using ProcessHacker.Common;
using ProcessHacker.Native;
using ProcessHacker.Native.Api;
using ProcessHacker.Native.Objects;
using ProcessHacker.Native.Security;

namespace ProcessHacker
{
    public partial class EditDEPWindow : Form
    {
        private readonly int _pid;

        public EditDEPWindow(int PID)
        {
            InitializeComponent();

            this.AddEscapeToClose();
            this.SetTopMost();

            _pid = PID;

            try
            {
                using (ProcessHandle phandle = new ProcessHandle(_pid, ProcessAccess.QueryInformation))
                {
                    var depStatus = phandle.DepStatus;
                    string str;

                    if ((depStatus & DepStatus.Enabled) != 0)
                    {
                        str = "Enabled";

                        if ((depStatus & DepStatus.AtlThunkEmulationDisabled) != 0)
                            str += ", DEP-ATL thunk emulation disabled";
                    }
                    else
                    {
                        str = "Disabled";
                    }

                    comboStatus.SelectedItem = str;

                    if (KProcessHacker2.Instance.KphIsConnected)
                        checkPermanent.Visible = true;
                }
            }
            catch
            { }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (KProcessHacker2.Instance.KphIsConnected)
                this.SetDepStatusKph();
            else
                this.SetDepStatusNoKph();
        }

        private void SetDepStatusKph()
        {
            DepStatus depStatus;

            switch (this.comboStatus.SelectedItem.ToString())
            {
                case "Disabled":
                    depStatus = 0;
                    break;
                case "Enabled":
                    depStatus = DepStatus.Enabled;
                    break;
                case "Enabled, DEP-ATL thunk emulation disabled":
                    depStatus = DepStatus.Enabled | DepStatus.AtlThunkEmulationDisabled;
                    break;
                default:
                    PhUtils.ShowError("Invalid value.");
                    return;
            }

            if (checkPermanent.Checked)
                depStatus |= DepStatus.Permanent;

            try
            {
                using (ProcessHandle phandle = new ProcessHandle(_pid, Program.MinProcessQueryRights))
                    phandle.DepStatus = depStatus;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                PhUtils.ShowException("Unable to set the DEP status", ex);
            }
        }

        private void SetDepStatusNoKph()
        {
            if (comboStatus.SelectedItem.ToString().StartsWith("Enabled", StringComparison.OrdinalIgnoreCase))
            {
                if (!PhUtils.ShowConfirmMessage(
                    "set",
                    "the DEP status",
                    "Enabling DEP in a process is a permanent action.",
                    false))
                    return;
            }

            DepFlags flags = DepFlags.Enable;

            if (comboStatus.SelectedItem.ToString() == "Disabled")
                flags = DepFlags.Disable;
            else if (comboStatus.SelectedItem.ToString() == "Enabled")
                flags = DepFlags.Enable;
            else if (comboStatus.SelectedItem.ToString() == "Enabled, DEP-ATL thunk emulation disabled")
                flags = DepFlags.Enable | DepFlags.DisableAtlThunkEmulation;
            else
            {
                PhUtils.ShowError("Invalid value.");
                return;
            }

            try
            {
                IntPtr kernel32 = Win32.GetModuleHandle("kernel32.dll");
                IntPtr setProcessDepPolicy = Win32.GetProcAddress(kernel32, "SetProcessDEPPolicy");

                if (setProcessDepPolicy == IntPtr.Zero)
                    throw new Exception("This feature is not supported on your version of Windows.");

                using (ProcessHandle phandle = new ProcessHandle(_pid,
                    Program.MinProcessQueryRights | ProcessAccess.VmOperation |
                    ProcessAccess.VmRead | ProcessAccess.CreateThread))
                {
                    var thread = phandle.CreateThreadWin32(setProcessDepPolicy, new IntPtr((int)flags));

                    thread.Wait(1000 * Win32.TimeMsTo100Ns);

                    int exitCode = thread.GetExitCode();

                    if (exitCode == 0)
                    {
                        throw new Exception("Unspecified error.");
                    }
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                PhUtils.ShowException("Unable to set the DEP status", ex);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
