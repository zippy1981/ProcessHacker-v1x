﻿using System;
using System.Collections.Generic;
using System.Text;
using ProcessHacker.Native.Api;
using ProcessHacker.Native.Objects;
using ProcessHacker.Native.Security;
using System.Threading;
using ProcessHacker.Native.Threading;
using System.Runtime.InteropServices;

namespace ProcessHacker.Native.SsLogging
{
    public delegate void ArgumentBlockReceivedDelegate(SsData argBlock);
    public delegate void EventBlockReceivedDelegate(SsEvent eventBlock);

    public sealed class SsLogger
    {
        private const int _highBlockSize = 0x200;

        internal static string ReadWString(MemoryRegion data)
        {
            KphSsWString wString = data.ReadStruct<KphSsWString>();

            return data.ReadUnicodeString(KphSsWString.BufferOffset, wString.Length / 2);
        }

        public event ArgumentBlockReceivedDelegate ArgumentBlockReceived;
        public event EventBlockReceivedDelegate EventBlockReceived;

        private bool _started = false;
        private object _startLock = new object();

        private bool _terminating = false;
        private Thread _bufferWorkerThread;
        private ThreadHandle _bufferWorkerThreadHandle;
        private Event _bufferWorkerThreadReadyEvent = new Event(true, false);

        private VirtualMemoryAlloc _buffer;
        private SemaphoreHandle _readSemaphore;
        private SemaphoreHandle _writeSemaphore;
        private KphSsClientEntryHandle _clientEntryHandle;
        private KphSsRuleSetEntryHandle _ruleSetEntryHandle;

        public SsLogger(int bufferedBlockCount, bool includeAll)
        {
            // Allocate a buffer.
            _buffer = new VirtualMemoryAlloc(_highBlockSize * bufferedBlockCount);

            // Create the read and write semaphores.

            // Read semaphore: no blocks initially, so 0 initial count.
            _readSemaphore = SemaphoreHandle.Create(SemaphoreAccess.All, 0, bufferedBlockCount);
            // Write semaphore: all buffer blocks available so max. initial count.
            _writeSemaphore = SemaphoreHandle.Create(SemaphoreAccess.All, bufferedBlockCount, bufferedBlockCount);

            // Create the client entry.
            _clientEntryHandle = KProcessHacker.Instance.SsCreateClientEntry(
                ProcessHandle.Current,
                _readSemaphore,
                _writeSemaphore,
                _buffer,
                _buffer.Size
                );

            // Create the ruleset entry.
            _ruleSetEntryHandle = KProcessHacker.Instance.SsCreateRuleSetEntry(
                _clientEntryHandle,
                includeAll ? KphSsFilterType.Include : KphSsFilterType.Exclude,
                KphSsRuleSetAction.Log
                );
        }

        private void BufferWorkerThreadStart()
        {
            int cursor = 0;

            // Open a handle to the current thread so other functions 
            // can alert us.
            _bufferWorkerThreadHandle = ThreadHandle.OpenCurrent(ThreadAccess.All);

            // We're ready.
            _bufferWorkerThreadReadyEvent.Set();

            while (!_terminating)
            {
                NtStatus status;
                KphSsBlockHeader blockHeader;

                // Wait for a block to read (enable alerting so we can 
                // be interrupted if someone wants us to stop).
                status = _readSemaphore.Wait(true);

                // Did we get alerted?
                if (status == NtStatus.Alerted)
                    return;

                // Check if we have an implicit cursor reset.
                if (_buffer.Size - cursor < Marshal.SizeOf(typeof(KphSsBlockHeader)))
                    cursor = 0;

                // Read the block header.
                blockHeader = _buffer.ReadStruct<KphSsBlockHeader>(cursor, 0);

                // Check if we have an explicit cursor reset.
                if (blockHeader.Type == KphSsBlockType.Reset)
                {
                    cursor = 0;
                    blockHeader = _buffer.ReadStruct<KphSsBlockHeader>(cursor, 0);
                }

                // Process the block.
                if (blockHeader.Type == KphSsBlockType.Event)
                {
                    var eventBlock = _buffer.ReadStruct<KphSsEventBlock>(cursor, 0);
                    int[] arguments;
                    IntPtr[] stackTrace;

                    // Reconstruct the argument and stack trace arrays.

                    arguments = new int[eventBlock.NumberOfArguments];
                    stackTrace = new IntPtr[eventBlock.TraceCount];

                    for (int i = 0; i < arguments.Length; i++)
                        arguments[i] = _buffer.ReadInt32(cursor + eventBlock.ArgumentsOffset, i);
                    for (int i = 0; i < stackTrace.Length; i++)
                        stackTrace[i] = _buffer.ReadIntPtr(cursor + eventBlock.TraceOffset, i);

                    // Create an event object.
                    SsEvent ssEvent = new SsEvent();

                    // Basic information
                    ssEvent.Time = DateTime.FromFileTime(eventBlock.Time);
                    ssEvent.ThreadId = eventBlock.ClientId.ThreadId;
                    ssEvent.ProcessId = eventBlock.ClientId.ProcessId;
                    ssEvent.Arguments = arguments;
                    ssEvent.StackTrace = stackTrace;

                    // Flags
                    ssEvent.ArgumentsCopyFailed =
                        (eventBlock.Flags & KphSsEventFlags.CopyArgumentsFailed) == KphSsEventFlags.CopyArgumentsFailed;
                    ssEvent.ArgumentsProbeFailed =
                        (eventBlock.Flags & KphSsEventFlags.ProbeArgumentsFailed) == KphSsEventFlags.ProbeArgumentsFailed;
                    ssEvent.CallNumber = eventBlock.Number;

                    if ((eventBlock.Flags & KphSsEventFlags.UserMode) == KphSsEventFlags.UserMode)
                        ssEvent.Mode = KProcessorMode.UserMode;
                    else
                        ssEvent.Mode = KProcessorMode.KernelMode;

                    // Raise the event.
                    if (this.EventBlockReceived != null)
                        this.EventBlockReceived(ssEvent);
                }
                else if (blockHeader.Type == KphSsBlockType.Argument)
                {
                    var argBlock = _buffer.ReadStruct<KphSsArgumentBlock>(cursor, 0);
                    SsData ssArg = null;

                    // Process the argument block based on its type.
                    switch (argBlock.Type)
                    {
                        case KphSsArgumentType.Int8:
                            {
                                SsSimple simpleArg = new SsSimple();

                                simpleArg.Argument = argBlock.Data.Int8;
                                simpleArg.Type = typeof(Byte);
                                ssArg = simpleArg;
                            }
                            break;
                        case KphSsArgumentType.Int16:
                            {
                                SsSimple simpleArg = new SsSimple();

                                simpleArg.Argument = argBlock.Data.Int16;
                                simpleArg.Type = typeof(Int16);
                                ssArg = simpleArg;
                            }
                            break;
                        case KphSsArgumentType.Int32:
                            {
                                SsSimple simpleArg = new SsSimple();

                                simpleArg.Argument = argBlock.Data.Int32;
                                simpleArg.Type = typeof(Int32);
                                ssArg = simpleArg;
                            }
                            break;
                        case KphSsArgumentType.Int64:
                            {
                                SsSimple simpleArg = new SsSimple();

                                simpleArg.Argument = argBlock.Data.Int64;
                                simpleArg.Type = typeof(Int64);
                                ssArg = simpleArg;
                            }
                            break;
                        case KphSsArgumentType.Handle:
                            {
                                ssArg = new SsHandle(
                                    new MemoryRegion(_buffer, cursor + KphSsArgumentBlock.DataOffset));
                            }
                            break;
                        case KphSsArgumentType.UnicodeString:
                            {
                                ssArg = new SsUnicodeString(
                                    new MemoryRegion(_buffer, cursor + KphSsArgumentBlock.DataOffset));
                            }
                            break;
                        case KphSsArgumentType.ObjectAttributes:
                            {
                                ssArg = new SsObjectAttributes(
                                    new MemoryRegion(_buffer, cursor + KphSsArgumentBlock.DataOffset));
                            }
                            break;
                    }

                    // Raise the event.
                    if (ssArg != null)
                    {
                        if (this.ArgumentBlockReceived != null)
                            this.ArgumentBlockReceived(ssArg);
                    }
                }

                // Advance the cursor.
                cursor += blockHeader.Size;
                // Signal that a buffer block is available for writing.
                _writeSemaphore.Release();
            }
        }

        public void Start()
        {
            lock (_startLock)
            {
                if (!_started)
                {
                    KProcessHacker.Instance.SsRef();
                    _started = true;

                    _terminating = false;

                    // Create the buffer worker thread.
                    _bufferWorkerThread = new Thread(this.BufferWorkerThreadStart);
                    _bufferWorkerThread.IsBackground = true;
                    _bufferWorkerThread.Start();
                    // Wait for the thread to initialize.
                    _bufferWorkerThreadReadyEvent.Wait();
                }
            }
        }

        public void Stop()
        {
            lock (_startLock)
            {
                if (_started)
                {
                    KProcessHacker.Instance.SsUnref();
                    _started = false;

                    // Tell the worker thread to stop.
                    _terminating = true;
                    // Alert it just in case it is waiting.
                    _bufferWorkerThreadHandle.Alert();
                    // Wait for the worker thread to terminate.
                    _bufferWorkerThreadHandle.Wait();
                    // Close the thread handle.
                    _bufferWorkerThreadHandle.Dispose();
                }
            }
        }
    }
}
