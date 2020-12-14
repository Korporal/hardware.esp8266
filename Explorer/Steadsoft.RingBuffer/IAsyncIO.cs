using System;


namespace Steadsoft.IO
{
    /// <summary>
    /// Provides access to an object's async begin/end methods for read and write operations.
    /// </summary>
    public interface IAsyncIO
    {
        IAsyncResult BeginRecv(byte[] buffer, int offset, int count, AsyncCallback callback, object state);
        int EndRecv(IAsyncResult Result);
        IAsyncResult BeginSend(byte[] buffer, int offset, int count, AsyncCallback callback, object state);
        void EndSend(IAsyncResult Result);
    }
}