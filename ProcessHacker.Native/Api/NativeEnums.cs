﻿/*
 * Process Hacker - 
 *   native API enumerations
 *                       
 * Copyright (C) 2009 Flavio Erlich
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
using System.Text;

namespace ProcessHacker.Native.Api
{
    [Flags]
    public enum ContextFlags : int
    {
        I386 = 0x00010000,
        I486 = 0x00010000,
        Control = I386 | 0x00000001,
        Integer = I386 | 0x00000002,
        Segments = I386 | 0x00000004,
        FloatingPoint = I386 | 0x00000008,
        DebugRegisters = I386 | 0x00000010,
        ExtendedRegisters = I386 | 0x00000020,
        Full = Control | Integer | Segments,
        All = Control | Integer | Segments | FloatingPoint | DebugRegisters | ExtendedRegisters
    }

    [Flags]
    public enum DebugObjectFlags : uint
    {
        KillOnClose = 0x1
    }

    [Flags]
    public enum DebugObjectInformationClass : int
    {
        DebugObjectFlags,
        MaxDebugObjectInfoClass
    }

    [Flags]
    public enum DbgState : int
    {
        DbgIdle,
        DbgReplyPending,
        DbgCreateThreadStateChange,
        DbgCreateProcessStateChange,
        DbgExitThreadStateChange,
        DbgExitProcessStateChange,
        DbgExceptionStateChange,
        DbgBreakpointStateChange,
        DbgSingleStepStateChange,
        DbgLoadDllStateChange,
        DbgUnloadDllStateChange
    }

    [Flags]
    public enum DuplicateOptions : int
    {
        CloseSource = 0x1,
        SameAccess = 0x2,
        SameAttributes = 0x4
    }

    public enum EventInformationClass : int
    {
        EventBasicInformation
    }

    public enum EventType : int
    {
        NotificationEvent,
        SynchronizationEvent
    }

    public enum FileAlignment : int
    {
        Byte = 0x0,
        Word = 0x1,
        Long = 0x3,
        Quad = 0x7,
        Octa = 0xf,
        ThirtyTwoByte = 0x1f,
        SixtyFourByte = 0x3f,
        OneHundredAndTwentyEightByte = 0x7f,
        TwoHundredAndFiftySixByte = 0xff,
        FiveHundredAndTwelveByte = 0x1ff
    }

    public enum FileInformationClass : int
    {
        FileDirectoryInformation = 1, // dir
        FileFullDirectoryInformation, // dir
        FileBothDirectoryInformation, // dir
        FileBasicInformation,
        FileStandardInformation,
        FileInternalInformation,
        FileEaInformation,
        FileAccessInformation,
        FileNameInformation,
        FileRenameInformation, // 10
        FileLinkInformation,
        FileNamesInformation, // dir
        FileDispositionInformation,
        FilePositionInformation,
        FileFullEaInformation,
        FileModeInformation,
        FileAlignmentInformation,
        FileAllInformation,
        FileAllocationInformation,
        FileEndOfFileInformation, // 20
        FileAlternateNameInformation,
        FileStreamInformation,
        FilePipeInformation,
        FilePipeLocalInformation,
        FilePipeRemoteInformation,
        FileMailslotQueryInformation,
        FileMailslotSetInformation,
        FileCompressionInformation,
        FileObjectIdInformation, // dir
        FileCompletionInformation, // 30
        FileMoveClusterInformation,
        FileQuotaInformation,
        FileReparsePointInformation,
        FileNetworkOpenInformation,
        FileAttributeTagInformation,
        FileTrackingInformation,
        FileIdBothDirectoryInformation, // dir
        FileIdFullDirectoryInformation, // dir
        FileValidDataLengthInformation,
        FileShortNameInformation, // 40
        FileMaximumInformation
    }

    public enum FileIoStatus : int
    {
        Superseded = 0,
        Opened = 1,
        Created = 2,
        Overwritten = 3,
        Exists = 4,
        DoesNotExist = 5
    }

    [Flags]
    public enum FileObjectFlags : int
    {
        FileOpen = 0x00000001,
        SynchronousIo = 0x00000002,
        AlertableIo = 0x00000004,
        NoIntermediateBuffering = 0x00000008,
        WriteThrough = 0x00000010,
        SequentialOnly = 0x00000020,
        CacheSupported = 0x00000040,
        NamedPipe = 0x00000080,
        StreamFile = 0x00000100,
        MailSlot = 0x00000200,
        GenerateAuditOnClose = 0x00000400,
        QueueIrpToThread = GenerateAuditOnClose,
        DirectDeviceOpen = 0x00000800,
        FileModified = 0x00001000,
        FileSizeChanged = 0x00002000,
        CleanupComplete = 0x00004000,
        TemporaryFile = 0x00008000,
        DeleteOnClose = 0x00010000,
        OpenedCaseSensitivity = 0x00020000,
        HandleCreated = 0x00040000,
        FileFastIoRead = 0x00080000,
        RandomAccess = 0x00100000,
        FileOpenCancelled = 0x00200000,
        VolumeOpen = 0x00400000,
        RemoteOrigin = 0x01000000,
        SkipCompletionPort = 0x02000000,
        SkipSetEvent = 0x04000000,
        SkipSetFastIo = 0x08000000
    }

    public enum FsInformationClass : int
    {
        FileFsVolumeInformation = 1,
        FileFsLabelInformation,
        FileFsSizeInformation,
        FileFsDeviceInformation,
        FileFsAttributeInformation, 
        FileFsControlInformation,
        FileFsFullSizeInformation,
        FileFsObjectIdInformation,
        FileFsDriverPathInformation,
        FileFsMaximumInformation
    }

    [Flags]
    public enum HandleFlags : byte
    {
        ProtectFromClose = 0x1,
        Inherit = 0x2,
        AuditObjectClose = 0x4
    }

    public enum HandleTraceType : int
    {
        Open = 1,
        Close = 2,
        BadRef = 3
    }

    [Flags]
    public enum HashStringAlgorithm : int
    {
        Default = 0,
        X65599 = 1,
        Invalid = -1
    }

    public enum IoCompletionInformationClass : int
    {
        IoCompletionBasicInformation
    }

    [Flags]
    public enum JobObjectBasicUiRestrictions : uint
    {
        Handles = 0x1,
        ReadClipboard = 0x2,
        WriteClipboard = 0x4,
        SystemParameters = 0x8,
        DisplaySettings = 0x10,
        GlobalAtoms = 0x20,
        Desktop = 0x40,
        ExitWindows = 0x80
    }

    public enum JobObjectInformationClass : int
    {
        JobObjectBasicAccountingInformation = 1,
        JobObjectBasicLimitInformation,
        JobObjectBasicProcessIdList,
        JobObjectBasicUIRestrictions,
        JobObjectSecurityLimitInformation,
        JobObjectEndOfJobTimeInformation,
        JobObjectAssociateCompletionPortInformation,
        JobObjectBasicAndIoAccountingInformation,
        JobObjectExtendedLimitInformation,
        JobObjectJobSetInformation
    }

    [Flags]
    public enum JobObjectLimitFlags : uint
    {
        WorkingSet = 0x1,
        ProcessTime = 0x2,
        JobTime = 0x4,
        ActiveProcess = 0x8,
        Affinity = 0x10,
        PriorityClass = 0x20,
        PreserveJobTime = 0x40,
        SchedulingClass = 0x80,
        ProcessMemory = 0x100,
        JobMemory = 0x200,
        DieOnUnhandledException = 0x400,
        BreakawayOk = 0x800,
        SilentBreakawayOk = 0x1000,
        KillOnJobClose = 0x2000,
    }

    [Flags]
    public enum KeyCreationDisposition : int
    {
        CreatedNewKey,
        OpenedExistingKey
    }

    public enum KeyInformationClass : int
    {
        KeyBasicInformation,
        KeyNodeInformation,
        KeyFullInformation,
        KeyNameInformation,
        KeyCachedInformation,
        KeyFlagsInformation,
        MaxKeyInfoClass
    }

    public enum KeySetInformationClass : int
    {
        KeyWriteTimeInformation,
        KeyUserFlagsInformation,
        MaxKeySetInfoClass
    }

    public enum KProfileSource : int
    {
        ProfileTime,
        ProfileAlignmentFixup,
        ProfileTotalIssues,
        ProfilePipelineDry,
        ProfileLoadInstructions,
        ProfilePipelineFrozen,
        ProfileBranchInstructions,
        ProfileTotalNonissues,
        ProfileDcacheMisses,
        ProfileIcacheMisses,
        ProfileCacheMisses,
        ProfileBranchMispredictions,
        ProfileStoreInstructions,
        ProfileFpInstructions,
        ProfileIntegerInstructions,
        Profile2Issue,
        Profile3Issue,
        Profile4Issue,
        ProfileSpecialInstructions,
        ProfileTotalCycles,
        ProfileIcacheIssues,
        ProfileDcacheAccesses,
        ProfileMemoryBarrierCycles,
        ProfileLoadLinkedIssues,
        ProfileMaximum
    }

    public enum KWaitReason : int
    {
        Executive = 0,
        FreePage = 1,
        PageIn = 2,
        PoolAllocation = 3,
        DelayExecution = 4,
        Suspended = 5,
        UserRequest = 6,
        WrExecutive = 7,
        WrFreePage = 8,
        WrPageIn = 9,
        WrPoolAllocation = 10,
        WrDelayExecution = 11,
        WrSuspended = 12,
        WrUserRequest = 13,
        WrEventPair = 14,
        WrQueue = 15,
        WrLpcReceive = 16,
        WrLpcReply = 17,
        WrVirtualMemory = 18,
        WrPageOut = 19,
        WrRendezvous = 20,
        Spare2 = 21,
        Spare3 = 22,
        Spare4 = 23,
        Spare5 = 24,
        WrCalloutStack = 25,
        WrKernel = 26,
        WrResource = 27,
        WrPushLock = 28,
        WrMutex = 29,
        WrQuantumEnd = 30,
        WrDispatchInt = 31,
        WrPreempted = 32,
        WrYieldExecution = 33,
        WrFastMutex = 34,
        WrGuardedMutex = 35,
        WrRundown = 36,
        MaximumWaitReason = 37
    }

    [Flags]
    public enum MemExecuteOptions : int
    {
        ExecuteDisable = 0x1,
        ExecuteEnable = 0x2,
        DisableThunkEmulation = 0x4,
        Permanent = 0x8
    }

    [Flags]
    public enum MemoryFlags : uint
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
        Release = 0x8000,
        Free = 0x10000,
        Private = 0x20000,
        Mapped = 0x40000,
        Reset = 0x80000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        Physical = 0x400000,
        LargePages = 0x20000000,
        DosLimit = 0x40000000,
        FourMbPages = 0x80000000
    }

    public enum MemoryInformationClass : int
    {
        MemoryBasicInformation,
        MemoryWorkingSetInformation,
        MemoryMappedFilenameInformation,
        MemoryRegionInformation,
        MemoryWorkingSetExInformation
    }

    [Flags]
    public enum MemoryProtection : uint
    {
        AccessDenied = 0x0,
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ExecuteWriteCopy = 0x80,
        Guard = 0x100,
        NoCache = 0x200,
        WriteCombine = 0x400,
        NoAccess = 0x01,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08
    }

    public enum MutantInformationClass : int
    {
        MutantBasicInformation
    }

    [Flags]
    public enum ObjectFlags : uint
    {
        Inherit = 0x2,
        Permanent = 0x10,
        Exclusive = 0x20,
        CaseInsensitive = 0x40,
        OpenIf = 0x80,
        OpenLink = 0x100,
        KernelHandle = 0x200,
        ForceAccessCheck = 0x400,
        ValidAttributes = 0x7f2
    }

    public enum ObjectInformationClass : int
    {
        ObjectBasicInformation = 0,
        ObjectNameInformation = 1,
        ObjectTypeInformation = 2,
        ObjectTypesInformation = 3,
        ObjectHandleFlagInformation = 4,
        ObjectSessionInformation = 5
    }

    public enum PipeEnd : int
    {
        Client = 0,
        Server = 1
    }

    public enum PipeCompletionMode : int
    {
        Queue = 0,
        Complete = 1
    }

    public enum PipeConfiguration : int
    {
        Inbound = 0,
        Outbound = 1,
        FullDuplex = 2
    }

    public enum PipeReadMode : int
    {
        ByteStream = 0,
        Message = 1
    }

    public enum PipeState : uint
    {
        Disconnected = 1,
        Listening = 2,
        Connected = 3,
        Closing = 4
    }

    public enum PipeType : int
    {
        ByteStream = 0,
        Message = 1
    }

    public enum PortMessageType : short
    {
        Request = 1,
        Reply = 2,
        Datagram = 3,
        LostReply = 4,
        PortClosed = 5,
        ClientDied = 6,
        Exception = 7,
        DebugEvent = 8,
        ErrorEvent = 9,
        ConnectionRequest = 10
    }

    [Flags]
    public enum PrivilegeSetFlags : int
    {
        AnyNecessary = 0,
        AllNecessary = 0x1
    }

    public enum ProcessInformationClass : int
    {
        ProcessBasicInformation, // 0
        ProcessQuotaLimits,
        ProcessIoCounters,
        ProcessVmCounters,
        ProcessTimes,
        ProcessBasePriority,
        ProcessRaisePriority,
        ProcessDebugPort,
        ProcessExceptionPort,
        ProcessAccessToken,
        ProcessLdtInformation, // 10
        ProcessLdtSize,
        ProcessDefaultHardErrorMode,
        ProcessIoPortHandlers,
        ProcessPooledUsageAndLimits,
        ProcessWorkingSetWatch,
        ProcessUserModeIOPL,
        ProcessEnableAlignmentFaultFixup,
        ProcessPriorityClass,
        ProcessWx86Information,
        ProcessHandleCount, // 20
        ProcessAffinityMask,
        ProcessPriorityBoost,
        ProcessDeviceMap,
        ProcessSessionInformation,
        ProcessForegroundInformation,
        ProcessWow64Information,
        ProcessImageFileName,
        ProcessLUIDDeviceMapsEnabled,
        ProcessBreakOnTermination,
        ProcessDebugObjectHandle, // 30
        ProcessDebugFlags,
        ProcessHandleTracing,
        ProcessIoPriority,
        ProcessExecuteFlags,
        ProcessResourceManagement,
        ProcessCookie,
        ProcessImageInformation,
        ProcessCycleTime,
        ProcessPagePriority,
        ProcessInstrumentationCallback, // 40
        ProcessThreadStackAllocation,
        ProcessWorkingSetWatchEx,
        ProcessImageFileNameWin32,
        ProcessImageFileMapping,
        ProcessAffinityUpdateMode,
        ProcessMemoryAllocationMode,
        MaxProcessInfoClass
    }

    [Flags]
    public enum RegHiveFormat : int
    {
        Standard = 0x1,
        Latest = 0x2,
        NoCompression = 0x4
    }

    [Flags]
    public enum RegNotifyFilter : int
    {
        Name = 0x1,
        Attributes = 0x2,
        LastSet = 0x4,
        Security = 0x8,
        Legal = Name | Attributes | LastSet | Security
    }

    [Flags]
    public enum RegOption : int
    {
        Reserved = 0x0,
        NonVolatile = 0x0,
        Volatile = 0x1,
        CreateLink = 0x2,
        BackupRestore = 0x4,
        OpenLink = 0x8,
        Legal = Reserved | NonVolatile | Volatile | CreateLink | BackupRestore | OpenLink
    }

    [Flags]
    public enum RegRestoreFlags : int
    {
        WholeHiveVolatile = 0x1,
        RefreshHive = 0x2,
        NoLazyFlush = 0x4,
        ForceRestore = 0x8
    }

    [Flags]
    public enum RegUnloadFlags : int
    {
        ForceUnload = 0x1
    }

    [Flags]
    public enum RtlDuplicateUnicodeStringFlags : int
    {
        NullTerminate = 0x1,
        AllocateNullString = 0x2
    }

    [Flags]
    public enum RtlUserProcessFlags : uint
    {
        ParamsNormalized = 0x00000001,
        ProfileUser = 0x00000002,
        ProfileKernel = 0x00000004,
        ProfileServer = 0x00000008,
        Reserve1Mb = 0x00000020,
        Reserve16Mb = 0x00000040,
        CaseSensitive = 0x00000080,
        DisableHeapDecommit = 0x00000100,
        DllRedirectionLocal = 0x00001000,
        AppManifestPresent = 0x00002000,
        ImageKeyMissing = 0x00004000,
        OptInProcess = 0x00020000
    }

    [Flags]
    public enum SectionAttributes : uint
    {
        Based = 0x200000,
        NoChange = 0x400000,
        File = 0x800000,
        Image = 0x1000000,
        Reserve = 0x4000000,
        Commit = 0x8000000,
        NoCache = 0x10000000,
        Global = 0x20000000,
        LargePages = 0x80000000
    }

    [Flags]
    public enum SectionInformationClass : int
    {
        SectionBasicInformation,
        SectionImageInformation
    }

    public enum SectionInherit : int
    {
        ViewShare = 1,
        ViewUnmap = 2
    }

    public enum SecurityImpersonationLevel : int
    {
        SecurityAnonymous,
        SecurityIdentification,
        SecurityImpersonation,
        SecurityDelegation
    }

    [Flags]
    public enum SecurityInformation : uint
    {
        Owner = 0x00000001,
        Group = 0x00000002,
        Dacl = 0x00000004,
        Sacl = 0x00000008,
        Label = 0x00000010,

        ProtectedDacl = 0x80000000,
        ProtectedSacl = 0x40000000,
        UnprotectedDacl = 0x20000000,
        UnprotectedSacl = 0x10000000
    }

    public enum SemaphoreInformationClass : int
    {
        SemaphoreBasicInformation
    }

    public enum SidAttributes : uint
    {
        Mandatory = 0x00000001,
        EnabledByDefault = 0x00000002,
        Enabled = 0x00000004,
        Owner = 0x00000008,
        UseForDenyOnly = 0x00000010,
        Integrity = 0x00000020,
        IntegrityEnabled = 0x00000040,
        LogonId = 0xc0000000,
        Resource = 0x20000000
    }

    public enum SidNameUse : int
    {
        User = 1,
        Group,
        Domain,
        Alias,
        WellKnownGroup,
        DeletedAccount,
        Invalid,
        Unknown,
        Computer,
        Label
    }

    [Flags]
    public enum SiRequested : uint
    {
        OwnerSecurityInformation = 0x1,
        GroupSecurityInformation = 0x2,
        DaclSecurityInformation = 0x4,
        SaclSecurityInformation = 0x8,
        LabelSecurityInformation = 0x10
    }

    public enum SystemInformationClass : int
    {
        SystemBasicInformation,
        SystemProcessorInformation,
        SystemPerformanceInformation,
        SystemTimeOfDayInformation,
        SystemPathInformation,
        SystemProcessInformation,
        SystemCallCountInformation,
        SystemDeviceInformation,
        SystemProcessorPerformanceInformation,
        SystemFlagsInformation,
        SystemCallTimeInformation, // 10
        SystemModuleInformation,
        SystemLocksInformation,
        SystemStackTraceInformation,
        SystemPagedPoolInformation,
        SystemNonPagedPoolInformation,
        SystemHandleInformation,
        SystemObjectInformation,
        SystemPageFileInformation,
        SystemVdmInstemulInformation,
        SystemVdmBopInformation, // 20
        SystemFileCacheInformation,
        SystemPoolTagInformation,
        SystemInterruptInformation,
        SystemDpcBehaviorInformation,
        SystemFullMemoryInformation,
        SystemLoadGdiDriverInformation,
        SystemUnloadGdiDriverInformation,
        SystemTimeAdjustmentInformation,
        SystemSummaryMemoryInformation,
        SystemMirrorMemoryInformation, // 30
        SystemPerformanceTraceInformation,
        SystemCrashDumpInformation,
        SystemExceptionInformation,
        SystemCrashDumpStateInformation,
        SystemKernelDebuggerInformation,
        SystemContextSwitchInformation,
        SystemRegistryQuotaInformation,
        SystemExtendServiceTableInformation, // used to be SystemLoadAndCallImage
        SystemPrioritySeparation,
        SystemVerifierAddDriverInformation, // 40
        SystemVerifierRemoveDriverInformation,
        SystemProcessorIdleInformation,
        SystemLegacyDriverInformation,
        SystemCurrentTimeZoneInformation,
        SystemLookasideInformation,
        SystemTimeSlipNotification,
        SystemSessionCreate,
        SystemSessionDetach,
        SystemSessionInformation,
        SystemRangeStartInformation, // 50
        SystemVerifierInformation,
        SystemVerifierThunkExtend,
        SystemSessionProcessInformation,
        SystemLoadGdiDriverInSystemSpace,
        SystemNumaProcessorMap,
        SystemPrefetcherInformation,
        SystemExtendedProcessInformation,
        SystemRecommendedSharedDataAlignment,
        SystemComPlusPackage,
        SystemNumaAvailableMemory, // 60
        SystemProcessorPowerInformation,
        SystemEmulationBasicInformation,
        SystemEmulationProcessorInformation,
        SystemExtendedHandleInformation,
        SystemLostDelayedWriteInformation,
        SystemBigPoolInformation,
        SystemSessionPoolTagInformation,
        SystemSessionMappedViewInformation,
        SystemHotpatchInformation,
        SystemObjectSecurityMode, // 70
        SystemWatchdogTimerHandler, // doesn't seem to be implemented
        SystemWatchdogTimerInformation,
        SystemLogicalProcessorInformation,
        SystemWow64SharedInformation,
        SystemRegisterFirmwareTableInformationHandler,
        SystemFirmwareTableInformation,
        SystemModuleInformationEx,
        SystemVerifierTriageInformation,
        SystemSuperfetchInformation,
        SystemMemoryListInformation, // 80
        SystemFileCacheInformationEx,
        SystemNotImplemented19,
        SystemProcessorDebugInformation,
        SystemVerifierInformation2,
        SystemNotImplemented20,
        SystemRefTraceInformation,
        SystemSpecialPoolTag, // MmSpecialPoolTag, then MmSpecialPoolCatchOverruns != 0
        SystemProcessImageName,
        SystemNotImplemented21,
        SystemBootEnvironmentInformation, // 90
        SystemEnlightenmentInformation,
        SystemVerifierInformationEx,
        SystemNotImplemented22,
        SystemNotImplemented23,
        SystemCovInformation,
        SystemNotImplemented24,
        SystemNotImplemented25,
        SystemPartitionInformation,
        SystemSystemDiskInformation, // this and SystemPartitionInformation both call IoQuerySystemDeviceName
        SystemPerformanceDistributionInformation, // 100
        SystemNumaProximityNodeInformation,
        SystemTimeZoneInformation2,
        SystemCodeIntegrityInformation,
        SystemNotImplemented26,
        SystemUnknownInformation, // No symbols for this case, very strange...
        SystemVaInformation // 106, calls MmQuerySystemVaInformation
    }

    public enum ThreadInformationClass : uint
    {
        ThreadBasicInformation,
        ThreadTimes,
        ThreadPriority,
        ThreadBasePriority,
        ThreadAffinityMask,
        ThreadImpersonationToken,
        ThreadDescriptorTableEntry,
        ThreadEnableAlignmentFaultFixup,
        ThreadEventPair,
        ThreadQuerySetWin32StartAddress,
        ThreadZeroTlsCell,
        ThreadPerformanceCount,
        ThreadAmILastThread,
        ThreadIdealProcessor,
        ThreadPriorityBoost,
        ThreadSetTlsArrayAddress,
        ThreadIsIoPending,
        ThreadHideFromDebugger,
        ThreadBreakOnTermination,
        ThreadSwitchLegacyState,
        ThreadIsTerminated,
        ThreadLastSystemCall,
        ThreadIoPriority,
        ThreadCycleTime,
        ThreadPagePriority,
        ThreadActualBasePriority,
        ThreadTebInformation,
        ThreadCSwitchMon,
        MaxThreadInfoClass
    }

    public enum TimerInformationClass : int
    {
        TimerBasicInformation
    }

    public enum TimerType : int
    {
        NotificationTimer,
        SynchronizationTimer
    }

    public enum TokenElevationType : int
    {
        Default = 1,
        Full,
        Limited
    }

    public enum TokenInformationClass
    {
        TokenUser = 1,
        TokenGroups,
        TokenPrivileges,
        TokenOwner,
        TokenPrimaryGroup,
        TokenDefaultDacl,
        TokenSource,
        TokenType,
        TokenImpersonationLevel,
        TokenStatistics, // 10
        TokenRestrictedSids,
        TokenSessionId,
        TokenGroupsAndPrivileges,
        TokenSessionReference,
        TokenSandBoxInert,
        TokenAuditPolicy,
        TokenOrigin,
        TokenElevationType,
        TokenLinkedToken,
        TokenElevation, // 20
        TokenHasRestrictions,
        TokenAccessInformation,
        TokenVirtualizationAllowed,
        TokenVirtualizationEnabled,
        TokenIntegrityLevel,
        TokenUIAccess,
        TokenMandatoryPolicy,
        TokenLogonSid,
        MaxTokenInfoClass  // MaxTokenInfoClass should always be the last enum
    }

    public enum TokenType : int
    {
        Primary = 1,
        Impersonation
    }

    public enum WaitType : int
    {
        WaitAll,
        WaitAny
    }

    public enum WellKnownSidType : int
    {
        WinNullSid = 0,
        WinWorldSid = 1,
        WinLocalSid = 2,
        WinCreatorOwnerSid = 3,
        WinCreatorGroupSid = 4,
        WinCreatorOwnerServerSid = 5,
        WinCreatorGroupServerSid = 6,
        WinNtAuthoritySid = 7,
        WinDialupSid = 8,
        WinNetworkSid = 9,
        WinBatchSid = 10,
        WinInteractiveSid = 11,
        WinServiceSid = 12,
        WinAnonymousSid = 13,
        WinProxySid = 14,
        WinEnterpriseControllersSid = 15,
        WinSelfSid = 16,
        WinAuthenticatedUserSid = 17,
        WinRestrictedCodeSid = 18,
        WinTerminalServerSid = 19,
        WinRemoteLogonIdSid = 20,
        WinLogonIdsSid = 21,
        WinLocalSystemSid = 22,
        WinLocalServiceSid = 23,
        WinNetworkServiceSid = 24,
        WinBuiltinDomainSid = 25,
        WinBuiltinAdministratorsSid = 26,
        WinBuiltinUsersSid = 27,
        WinBuiltinGuestsSid = 28,
        WinBuiltinPowerUsersSid = 29,
        WinBuiltinAccountOperatorsSid = 30,
        WinBuiltinSystemOperatorsSid = 31,
        WinBuiltinPrintOperatorsSid = 32,
        WinBuiltinBackupOperatorsSid = 33,
        WinBuiltinReplicatorSid = 34,
        WinBuiltinPreWindows2000CompatibleAccessSid = 35,
        WinBuiltinRemoteDesktopUsersSid = 36,
        WinBuiltinNetworkConfigurationOperatorsSid = 37,
        WinAccountAdministratorSid = 38,
        WinAccountGuestSid = 39,
        WinAccountKrbtgtSid = 40,
        WinAccountDomainAdminsSid = 41,
        WinAccountDomainUsersSid = 42,
        WinAccountDomainGuestsSid = 43,
        WinAccountComputersSid = 44,
        WinAccountControllersSid = 45,
        WinAccountCertAdminsSid = 46,
        WinAccountSchemaAdminsSid = 47,
        WinAccountEnterpriseAdminsSid = 48,
        WinAccountPolicyAdminsSid = 49,
        WinAccountRasAndIasServersSid = 50,
        WinNTLMAuthenticationSid = 51,
        WinDigestAuthenticationSid = 52,
        WinSChannelAuthenticationSid = 53,
        WinThisOrganizationSid = 54,
        WinOtherOrganizationSid = 55,
        WinBuiltinIncomingForestTrustBuildersSid = 56,
        WinBuiltinPerfMonitoringUsersSid = 57,
        WinBuiltinPerfLoggingUsersSid = 58,
        WinBuiltinAuthorizationAccessSid = 59,
        WinBuiltinTerminalServerLicenseServersSid = 60,
        WinBuiltinDCOMUsersSid = 61,
        WinBuiltinIUsersSid = 62,
        WinIUserSid = 63,
        WinBuiltinCryptoOperatorsSid = 64,
        WinUntrustedLabelSid = 65,
        WinLowLabelSid = 66,
        WinMediumLabelSid = 67,
        WinHighLabelSid = 68,
        WinSystemLabelSid = 69,
        WinWriteRestrictedCodeSid = 70,
        WinCreatorOwnerRightsSid = 71,
        WinCacheablePrincipalsGroupSid = 72,
        WinNonCacheablePrincipalsGroupSid = 73,
        WinEnterpriseReadonlyControllersSid = 74,
        WinAccountReadonlyControllersSid = 75,
        WinBuiltinEventLogReadersGroup = 76,
        WinNewEnterpriseReadonlyControllersSid = 77,
        WinBuiltinCertSvcDComAccessGroup = 78
    }
}
