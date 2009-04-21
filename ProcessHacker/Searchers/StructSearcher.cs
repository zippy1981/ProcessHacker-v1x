﻿/*
 * Process Hacker - 
 *   struct searcher
 * 
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

using System;
using System.Runtime.InteropServices;
using ProcessHacker.Native.Api;
using ProcessHacker.Native.Objects;
using ProcessHacker.Structs;

namespace ProcessHacker
{
    public class StructSearcher : Searcher
    {
        public StructSearcher(int PID) : base(PID) { }

        public override void Search()
        {
            Results.Clear();

            ProcessHandle phandle;
            int address = 0;
            MemoryBasicInformation info = new MemoryBasicInformation();
            int count = 0;

            bool opt_priv = (bool)Params["private"];
            bool opt_img = (bool)Params["image"];
            bool opt_map = (bool)Params["mapped"];

            string structName = (string)Params["struct"];
            int align = (int)BaseConverter.ToNumberParse((string)Params["struct_align"]);

            if (!Program.Structs.ContainsKey(structName))
            {
                CallSearchError("Struct '" + structName + "' is not defined.");
                return;
            }

            StructDef structDef = Program.Structs[structName];
            string structLen = structDef.Size.ToString();

            structDef.IOProvider = new ProcessMemoryIO(PID);

            try
            {
                phandle = new ProcessHandle(PID, ProcessHacker.Native.Security.ProcessAccess.QueryInformation);
            }
            catch
            {
                CallSearchError("Could not open process: " + Win32.GetLastErrorMessage());
                return;
            }

            while (true)
            {
                if (!Win32.VirtualQueryEx(phandle, address, ref info,
                    Marshal.SizeOf(typeof(MemoryBasicInformation))))
                {
                    break;
                }
                else
                {
                    address += info.RegionSize;

                    // skip unreadable areas
                    if (info.Protect == MemoryProtection.AccessDenied)
                        continue;
                    if (info.State != MemoryState.Commit)
                        continue;

                    if ((!opt_priv) && (info.Type == MemoryType.Private))
                        continue;

                    if ((!opt_img) && (info.Type == MemoryType.Image))
                        continue;

                    if ((!opt_map) && (info.Type == MemoryType.Mapped))
                        continue;

                    CallSearchProgressChanged(
                        String.Format("Searching 0x{0:x8} ({1} found)...", info.BaseAddress, count));

                    for (int i = 0; i < info.RegionSize; i += align)
                    {
                        try
                        {
                            structDef.Offset = info.BaseAddress + i;
                            structDef.Read();
                            
                            // read succeeded, add it to the results
                            Results.Add(new string[] { String.Format("0x{0:x8}", info.BaseAddress),
                                String.Format("0x{0:x8}", i), structLen, "" });
                            count++;
                        }
                        catch
                        { }
                    }
                }
            }

            phandle.Dispose();

            CallSearchFinished();
        }
    }
}
