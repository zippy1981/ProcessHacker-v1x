﻿using System;

namespace ProcessHacker.Native.Security
{
    [Flags]
    public enum TimerAccess : uint
    {
        QueryState = 0x1,
        ModifyState = 0x2,
        All = 0x1f0003
    }
}