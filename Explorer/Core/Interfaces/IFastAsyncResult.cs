using Steadsoft.Devices.WiFi.ESP8266;
using System;

namespace Steadsoft.Devices
{
    public interface IFastAsyncResult : IAsyncResult
    {
        int BytesTransferred { get; }
        RingBuffer Buffer { get; }
    }
}
