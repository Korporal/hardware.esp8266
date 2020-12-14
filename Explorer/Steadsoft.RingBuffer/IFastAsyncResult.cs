using System;

namespace Steadsoft.IO
{
    public interface IFastAsyncResult : IAsyncResult
    {
        int BytesTransferred { get; }
        RingBuffer Buffer { get; }
    }
}
