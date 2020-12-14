using Steadsoft.Utility;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Steadsoft.IO.FastIODelegates;

[assembly: InternalsVisibleTo("RingBufferTests")]

namespace Steadsoft.IO
{
    /// <summary>
    /// Represents a fixed capacity ring buffer of bytes that may be continuously written to and read from.
    /// The ring buffer is not designed to be thread safe.
    /// </summary>
    /// <remarks>
    /// The ring buffer class facilitates the design of simple algorithms for handling the receipt and
    /// processing of asynchronously received data. Asynchronous receivers can simply append received
    /// data to the ring buffer without regard to details of buffer wrapping. So long as data is removed at 
    /// approx the same rate as it arrives a ring buffer of sufficient capacity will never overflow.
    /// Data within the ring buffer may examined by either "reading" (removal) or "peeking" (non destructive
    /// reading) the ability to peek enables applications to examine data for certain patterns in such a 
    /// way that the data is only removed (read) once all of the data comprising the pattern (detected by successive peeking)
    /// has been written to the buffer.
    /// Your application's asynchronous callback simply writes received data and then "peeks" the buffer to 
    /// see if sufficient data has yet been received to enable further processing.
    /// </remarks>
    public sealed class RingBuffer : IRingBufferLog
    {
        private int read_offset;
        private int write_offset;
        private ulong bytes_written;
        private ulong bytes_read;
        private readonly ArrayList log;
        /// <summary>
        /// Total number of bytes that have been written to the buffer since it was created or last cleared.
        /// </summary>
        public ulong NumberOfBytesWritten { get { return bytes_written; } }
        /// <summary>
        /// Total number of bytes that have been read from the buffer since it was created or last cleared.
        /// </summary>
        public ulong NumberOfBytesRead { get { return bytes_read; } }
        /// <summary>
        /// The total number of bytes that can be stored in the ring buffer.
        /// </summary>
        public int Capacity { get; private set; }
        /// <summary>
        /// The gretest number of bytes that have been stored in the ring buffer since it was created.
        /// </summary>
        public int PeakLength { get; private set; }
        /// <summary>
        /// Indicates of buffer logging is enabled.
        /// </summary>
        public bool Logging { get; private set; }
        internal byte[] Buffer { get; }
        /// <summary>
        /// Provides a means of enumerating all unread bytes.
        /// </summary>
        /// <remarks>
        /// This property exposes unread content as a sequence, masking any internal wrapping that 
        /// may be present within the buffer itself. The enumeration of the data begins at the 
        /// specified start offset into the unread data.
        /// </remarks>
        public IEnumerable Content(int StartOffset = 0)
        {
            if (Empty)
                yield break;

            if (StartOffset >= UsedBytes)
                throw new ArgumentOutOfRangeException($"The start offset must be less than the used space.");

            int start = (read_offset + StartOffset) % Capacity;

            for (int I = start; I < read_offset + ContiguousUsedSpace; I++)
            {
                yield return Buffer[I];
            }

            for (int I = 0; I < write_offset; I++)
            {
                yield return Buffer[I];
            }
        }

        public bool Contains(params byte[] Data)
        {

            Data.ThrowIfNull(nameof(Data));
            Data.Length.ThrowIfLessThanOrEqualToZero(nameof(Data.Length));

            if (Data.Length > UsedBytes)
                return false;

            int data_idx = 0;

            foreach (byte C in Content())
            {
            reset:
                if (C == Data[data_idx])
                {
                    data_idx++;

                    if (data_idx == Data.Length)
                        return true;
                }
                else
                {
                    if (data_idx > 0)
                    {
                        data_idx = 0;
                        goto reset;
                    }
                }
            }

            return false;

        }

        public bool BeginsWith(params byte[] Data)
        {
            Data.ThrowIfNull(nameof(Data));
            Data.Length.ThrowIfLessThanOrEqualToZero(nameof(Data.Length));

            if (Data.Length > UsedBytes)
                return false;

            int data_idx = 0;
            bool found = true; // assume true, exit if we find a mismatch.

            foreach (byte C in Content())
            {
                if (C != Data[data_idx])
                {
                    found = false;
                    break;
                }

                data_idx++;

                if (data_idx == Data.Length)
                    break;
            }

            return found;
        }

        /// <summary>
        /// Creates a ring buffer that can store a maximum of 'Capacity' bytes.
        /// </summary>
        /// <param name="Capacity"></param>
        public RingBuffer(int Capacity)
        {
            if (Capacity <= 0)
                throw new ArgumentException("The value must be greater than zero.", nameof(Capacity));

            Buffer = new byte[Capacity];
            this.Capacity = Capacity;
            log = new ArrayList();
        }
        /// <summary>
        /// Resets the ring buffer to it's initial state, any unread data stored in the buffer is lost.
        /// </summary>
        public void Clear()
        {
            read_offset = 0;
            write_offset = 0;
            bytes_read = 0;
            bytes_written = 0;
        }
        /// <summary>
        /// Copies the entire supplied byte array into the ring buffer providing there is sufficient free space.
        /// </summary>
        /// <param name="Data"></param>
        public void Write(params byte[] Data)
        {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
            Data.ThrowIfNull(nameof(Data));
            Write(Data, Data.Length);
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
        }
        /// <summary>
        /// Copies the requested number of bytes from the ring buffer providing there is sufficient used space.
        /// </summary>
        /// <param name="Length"></param>
        /// <returns></returns>
        public byte[] Read(int Length)
        {
            if (Empty)
                throw new InvalidOperationException("Cannot read data from an already empty ring buffer.");

#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
            try
            {
                return Read(Length, true);

            }
            finally
            {
#if INTEGRITY_CHECKING
                IntegrityCheck();
#endif

            }

        }
        /// <summary>
        /// Copies the specified number of bytes from supplied byte array into the ring buffer providing there is sufficient free space.
        /// </summary>
        /// <param name="Data"></param>
        public void Write(byte[] Data, int LengthToWrite)
        {

#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            Data.ThrowIfNull(nameof(Data));
            Data.Length.ThrowIfZero(nameof(Data.Length));
            LengthToWrite.ThrowIfLessThanZero(nameof(LengthToWrite));
            LengthToWrite.ThrowIfGreaterThan(Data.Length, nameof(LengthToWrite));
            LengthToWrite.ThrowIfGreaterThan(FreeBytes, nameof(LengthToWrite));

            // We rely on one or perhaps two copies and use the FastWrite operation.

            int local_index = 0;

            if (LengthToWrite > ContiguousFreeSpace)
            {
                CopyBytes(Data, 0, Buffer, write_offset, ContiguousFreeSpace);
                local_index += ContiguousFreeSpace;
                LengthToWrite = LengthToWrite - ContiguousFreeSpace;
                FinishFastRecv(ContiguousFreeSpace);
#if INTEGRITY_CHECKING
                IntegrityCheck();
#endif
            }

            CopyBytes(Data, local_index, Buffer, write_offset, LengthToWrite);
            FinishFastRecv(LengthToWrite);
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
        }
        /// <summary>
        /// Copies the requested number of bytes from the ring buffer providing there is sufficient used space, the buffer is not updated.
        /// </summary>
        /// <param name="Length"></param>
        /// <returns></returns>
        public byte[] Peek(int Length)
        {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
            return Read(Length, false);
        }
        public byte[] PeekLog(int Length)
        {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            if (!Logging)
                throw new InvalidOperationException("The ring buffer is not currently logging.");

            if (Length <= 0)
                throw new ArgumentException("The value must be greater than zero.", nameof(Length));

            byte[] temp = new byte[Length];
            log.CopyTo(0, temp, 0, Length);
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            return temp;
        }
        public byte[] PeekLog()
        {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            if (!Logging)
                throw new InvalidOperationException("The ring buffer is not currently logging.");
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            return (byte[])(log.ToArray(typeof(byte)));
        }
        public void StartLogging()
        {
            if (Logging)
                throw new InvalidOperationException("The ring buffer is already logging.");

            Logging = true;
        }
        public void StopLogging()
        {
            if (!Logging)
                throw new InvalidOperationException("The ring buffer is not currently logging.");

            Logging = false;
        }
        public int LogLength
        {
            get
            {
                if (!Logging)
                    throw new InvalidOperationException("The ring buffer is not currently logging.");

                return log.Count;
            }
        }
        public void ClearLog()
        {
            log.Clear();
        }

        /// <summary>
        /// Scans the current buffered (unread) data to see if the specified pattern
        /// is present and if so returns the length required to read the data up to and
        /// including the pattern bytes.
        /// </summary>
        /// <param name="MatchedLength">If a match is found this is the number of bytes that must be read to get the matched block.</param>
        /// <param name="Pattern">A sequence of one or more bytes.</param>
        /// <returns></returns>
        public bool TryFindBytes(out int MatchedLength, params byte[] Pattern)
        {
            Pattern = Pattern ?? throw new ArgumentNullException(nameof(Pattern));

            if (Pattern.Length == 0)
                throw new ArgumentException("The array must not be empty.", nameof(UsedBytes));

            MatchedLength = 0;

            if (Pattern.Length > UsedBytes)
                return false;

            if (this.UsedBytes == 0)
                return false;

            // This not my code, it was found on the web and is better than my own first attempt.

            var buffer = Peek(this.UsedBytes);

            int resumeIndex;

            for (int i = 0; i <= buffer.Length - Pattern.Length; i++)
            {
                if (buffer[i] == Pattern[0]) // Current byte equals first byte of pattern
                {
                    if (Pattern.Length == 1)
                    {
                        MatchedLength = i + Pattern.Length;
                        return true;
                    }

                    resumeIndex = 0;
                    for (int x = 1; x < Pattern.Length; x++)
                    {
                        if (buffer[i + x] == Pattern[x])
                        {
                            if (x == Pattern.Length - 1)  // Matched the entire pattern
                            {
                                MatchedLength = i + Pattern.Length;
                                return true;

                            }
                            else if (resumeIndex == 0 && buffer[i + x] == Pattern[0])  // The current byte equals the first byte of the pattern so start here on the next outer loop iteration
                                resumeIndex = i + x;
                        }
                        else
                        {
                            if (resumeIndex > 0)
                                i = resumeIndex - 1;  // The outer loop iterator will increment so subtract one
                            else if (x > 1)
                                i += (x - 1);  // Advance the outer loop variable since we already checked these bytes
                            break;
                        }
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// The number of unread bytes that have been written to the ring buffer.
        /// </summary>
        public int UsedBytes
        {
            get
            {
#if INTEGRITY_CHECKING
                IntegrityCheck();
#endif
                return (int)(bytes_written - bytes_read);
            }
        }
        /// <summary>
        /// The number of bytes available for writing data.
        /// </summary>
        public int FreeBytes
        {
            get
            {
#if INTEGRITY_CHECKING
                IntegrityCheck();
#endif
                return Capacity - UsedBytes;
            }
        }
        private byte[] Read(int LengthToRead, bool Consume)
        {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            LengthToRead.ThrowIfZero(nameof(LengthToRead));
            LengthToRead.ThrowIfLessThanZero(nameof(LengthToRead));
            LengthToRead.ThrowIfGreaterThan(UsedBytes, nameof(LengthToRead));

            int temp_read_offset;
            ulong temp_bytes_read;

            byte[] Data = new byte[LengthToRead];

            int local_index = 0;

            bool two_part_copy = false;

            if (LengthToRead > ContiguousUsedSpace)
            {
                two_part_copy = true;

                CopyBytes(Buffer, read_offset, Data, 0, ContiguousUsedSpace);
                local_index += ContiguousUsedSpace;
                LengthToRead = LengthToRead - ContiguousUsedSpace;

                if (Consume)
                    FinishFastSend(ContiguousUsedSpace);

#if INTEGRITY_CHECKING
                IntegrityCheck();
#endif

            }

            temp_read_offset = read_offset;
            temp_bytes_read = bytes_read;

            if (!Consume && two_part_copy)
            {
                // Because 'consume' is false we will not have called FinishFastSend if we executed the 
                // block above. Because of that the ContiguousReadPosition will not reflect the read
                // and the second copy below would misbehave if it relied on ContiguousReadPosition.
                // This is why we have a 'simulated' ContiguousReadPosition (temp_read_position)
                // who's valued can be updated as if we did call FinishFastSend.

                IncrementWrappableOffset(ContiguousUsedSpace, ref temp_read_offset, ref temp_bytes_read);
            }

            CopyBytes(Buffer, temp_read_offset, Data, local_index, LengthToRead);

            if (Consume)
                FinishFastSend(LengthToRead);

#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            return Data;
        }
        /// <summary>
        /// Updates the ring buffer state to reflect the fact that the specified number of bytes have been written.
        /// </summary>
        /// <remarks>
        /// Fast writing refers to the technique of allowing data to be written directly into the buffer by
        /// (for example) an async IO callback. This is performed by always writing to the contiguous free
        /// space in the buffer. This operation always consists of the direct buffer write followed by a 
        /// call to this method, which updates the state to reflect the newly written data.
        /// </remarks>
        /// <param name="Length"></param>
        internal void FinishFastSend(int Length)
        {
            if (Empty)
                throw new InvalidOperationException("Cannot write data to an already full ring buffer.");

#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
            Length.ThrowIfLessThanOrEqualToZero(nameof(Length));
            Length.ThrowIfGreaterThan(ContiguousUsedSpace, nameof(Length));

            IncrementWrappableOffset(Length, ref read_offset, ref bytes_read);

            if (this.UsedBytes > PeakLength)
                PeakLength = this.UsedBytes;

#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
        }
        internal void FinishFastRecv(int Length)
        {
            if (Full)
                throw new InvalidOperationException("Cannot read data from an already empty ring buffer.");


#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
            Length.ThrowIfLessThanOrEqualToZero(nameof(Length));
            Length.ThrowIfGreaterThan(ContiguousFreeSpace, nameof(Length));

            IncrementWrappableOffset(Length, ref write_offset, ref bytes_written);

#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

        }
        /// <summary>
        /// The number of bytes that can be written without wrapping.
        /// </summary>
        internal int ContiguousFreeSpace
        {
            get
            {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

                try
                {
                    if (Full)
                        return 0;

                    if (Empty)
                        return Capacity - write_offset;

                    if (write_offset > read_offset)
                        return Capacity - write_offset;

                    if (read_offset > write_offset)
                        return read_offset - write_offset;

                    throw new InvalidOperationException("The buffer read and write offsets are the same yet the buffer is neither full nor empty.");

                }
                finally
                {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
                }
            }
        }
        /// <summary>
        /// The number of bytes that can be read without wrapping.
        /// </summary>
        internal int ContiguousUsedSpace
        {
            get
            {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

                try
                {
                    if (Empty)
                        return 0;

                    if (Full)
                        return Capacity - read_offset;

                    if (read_offset > write_offset)
                        return Capacity - read_offset;

                    if (write_offset > read_offset)
                        return write_offset - read_offset;

                    throw new InvalidOperationException("The buffer read and write offsets are the same yet the buffer is neither full nor empty.");

                }
                finally
                {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
                }
            }
        }
        /// <summary>
        /// The offset at which a contiguous direct write to the buffer should start.
        /// </summary>
        /// <summary>
        /// Begins a fast read operation that writes directly into the ring buffer avoding costly buffer copying.
        /// </summary>
        /// <param name="AsyncObject">An object that exposes async IO methods.</param>
        /// <param name="CompletionCallback">A callback in the caller that is called when the async begin completes.</param>
        /// <param name="State">An arbitrary object that becomes available to the caller when their completion callback is invoked.</param>
        /// <remarks>
        /// This method is called to begin an async read with automatic fast writing of the received data into the ring buffer.
        /// Callers do not need to call an 'EndXXX' method because this is handled internally. When the caller's callback is
        /// eventually invoked they can assume that async operation has completed and the data has been written into the buffer.
        /// </remarks>
        /// <returns></returns>
        public IAsyncResult BeginFastRecv(IAsyncIO AsyncObject, FastAsyncCallback CompletionCallback, object State = null)
        {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            try
            {
                AsyncObject = AsyncObject ?? throw new ArgumentNullException(nameof(AsyncObject));
                CompletionCallback = CompletionCallback ?? throw new ArgumentNullException(nameof(CompletionCallback));

                var wrapper = new FastIOContext() { CallersCallback = CompletionCallback, CallersEndReadCallback = AsyncObject.EndRecv, CallersState = State };

                // Create a new async result that contains the original caller's state.

                var result = AsyncObject.BeginRecv(Buffer, write_offset, ContiguousFreeSpace, FastRecvBufferWriteCallback, wrapper);
                var callersResult = FastAsyncResult.Create(result, this, State);
                return callersResult;

            }
            finally
            {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
            }
        }
        /// <summary>
        /// Internal callback that performs buffer updating and user state data unwrapping then calls the caller's callback.
        /// </summary>
        /// <param name="result">An async result containing the same data as if the caller had called the end callback directly.</param>
        private void FastRecvBufferWriteCallback(IAsyncResult result)
        {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            try
            {
                var context = (FastIOContext)(result.AsyncState);
                int bytesRead = context.CallersEndReadCallback(result);

                FinishFastRecv(bytesRead);
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

                // Create a new async result that unwraps the original caller's state.

                var callersResult = FastAsyncResult.Create(result, this, context.CallersState, bytesRead);
                context.CallersCallback(callersResult);

            }
            finally
            {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
            }
        }
        public IAsyncResult BeginFastSend(IAsyncIO AsyncObject, FastAsyncCallback CompletionCallback, object State = null)
        {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            try
            {
                AsyncObject = AsyncObject ?? throw new ArgumentNullException(nameof(AsyncObject));
                CompletionCallback = CompletionCallback ?? throw new ArgumentNullException(nameof(CompletionCallback));

                var wrapper = new FastIOContext() { CallersCallback = CompletionCallback, CallersEndWriteCallback = AsyncObject.EndSend, CallersState = State, BytesToWrite = ContiguousUsedSpace };

                // Create a new async result that contains the original caller's state.

                var result = AsyncObject.BeginSend(Buffer, read_offset, ContiguousUsedSpace, FastSendBufferReadCallback, wrapper);
                var callersResult = FastAsyncResult.Create(result, this, State);
                return callersResult;

            }
            finally
            {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
            }
        }
        /// <summary>
        /// Internal callback that performs buffer updating and user state data unwrapping then calls the caller's callback.
        /// </summary>
        /// <param name="result">An async result containing the same data as if the caller had called the end callback directly.</param>
        private void FastSendBufferReadCallback(IAsyncResult result)
        {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            try
            {
                var context = (FastIOContext)(result.AsyncState);
                context.CallersEndWriteCallback(result);

                FinishFastRecv(context.BytesToWrite);

                // Create a new async result that unwraps the original caller's state.

                var callersResult = FastAsyncResult.Create(result, this, context.CallersState, context.BytesToWrite);
                context.CallersCallback(callersResult);

            }
            finally
            {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
            }
        }
        /// <summary>
        /// Indicates if there is any used space in the buffer.
        /// </summary>
        public bool Empty { get { return bytes_read == bytes_written; } }
        /// <summary>
        /// Indicates if there is any free space in the buffer.
        /// </summary>
        public bool Full { get { return bytes_written - bytes_read == (ulong)Capacity; } }
        /// <summary>
        /// Increments the supplied offset by the supplied number of bytes.
        /// </summary>
        /// <remarks>
        /// This operation increments an offset that points into the buffer data array. It automatically 'wraps' the offset
        /// in necessary and the caller has no need to be concerned with buffer wrapping. This operation is intended to
        /// adjust the offset and adjust the byte count, simplifying the nature of the calling code.
        /// </remarks>
        /// <param name="NumberOfBytes">Number of bytes to increment the offset by.</param>
        /// <param name="Offset">An existing offset into the buffers data array.</param>
        /// <param name="Bytes">An existing count of the number of bytes referred to by the offset.</param>
        private void IncrementWrappableOffset(int NumberOfBytes, ref int Offset, ref ulong Bytes) // adjusting this position means we must also adjust the bytes read/written...here, now, else...
        {
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif

            try
            {
                NumberOfBytes.ThrowIfGreaterThan(Capacity, nameof(Offset));

                if (Offset + NumberOfBytes < Capacity)
                {
                    Offset += NumberOfBytes;
                }
                else
                {
                    Offset = (Offset + NumberOfBytes) % Capacity;
                }
            }
            finally
            {
                Bytes += (ulong)NumberOfBytes;
#if INTEGRITY_CHECKING
            IntegrityCheck();
#endif
            }
        }
        private void IntegrityCheck()
        {
            if (write_offset == read_offset)
                if (!Full && !Empty)
                    Debugger.Break();
        }
        /// <summary>
        /// Internal helper that simply throws improved exception messages when an illegcal copy is attempted.
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="SourceOffset"></param>
        /// <param name="Target"></param>
        /// <param name="TargetOffset"></param>
        /// <param name="LengthToCopy"></param>
        private static void CopyBytes(Array Source, int SourceOffset, Array Target, int TargetOffset, int LengthToCopy)
        {
            if (Source.Length - SourceOffset < LengthToCopy)
                throw new ArgumentOutOfRangeException("The source buffer has insufficient space to satisfy the copy request.");

            if (Target.Length - TargetOffset < LengthToCopy)
                throw new ArgumentOutOfRangeException("The target buffer has insufficient space to satisfy the copy request.");

            System.Buffer.BlockCopy(Source, SourceOffset, Target, TargetOffset, LengthToCopy);

        }
        private class FastIOContext
        {
            public object CallersState { get; internal set; }
            public int BytesToWrite { get; internal set; }
            public FastAsyncCallback CallersCallback { get; internal set; }
            public EndFastReadDelegate CallersEndReadCallback { get; internal set; }
            public EndFastWriteDelegate CallersEndWriteCallback { get; internal set; }
        }
    }
}