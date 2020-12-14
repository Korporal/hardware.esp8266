using System;

namespace Steadsoft.IO
{
    public static class FastIODelegates
    {
        public delegate IAsyncResult BeginFastIODelegate(byte[] buffer, int offset, int count, AsyncCallback callback, object state);
        public delegate int EndFastReadDelegate(IAsyncResult Result);
        public delegate void EndFastWriteDelegate(IAsyncResult Result);
        public delegate void FastAsyncCallback(IFastAsyncResult Result);
    }
}