using System;
using System.Threading;

namespace Steadsoft.IO
{
    public class FastAsyncResult : IFastAsyncResult
    {
        public bool IsCompleted { get; private set; }

        public WaitHandle AsyncWaitHandle { get; private set; }

        public object AsyncState { get; private set; }

        public bool CompletedSynchronously { get; private set; }
        public int BytesTransferred { get; private set; }

        public RingBuffer Buffer { get; private set; }

    internal static FastAsyncResult Create (IAsyncResult Result, RingBuffer Buffer, object CallersState, int Bytes = 0)
        {
            var callersResult = new FastAsyncResult()
            {
                AsyncState = CallersState,
                AsyncWaitHandle = Result.AsyncWaitHandle,
                CompletedSynchronously = Result.CompletedSynchronously,
                IsCompleted = Result.IsCompleted,
                Buffer = Buffer,
                BytesTransferred = Bytes
            };

            return callersResult;
        }
    }
}
