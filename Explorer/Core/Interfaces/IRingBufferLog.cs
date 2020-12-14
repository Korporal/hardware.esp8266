namespace Steadsoft.Devices.WiFi.ESP8266
{
    /// <summary>
    /// Represents anything that contains a ring buffer and wishes to expose its logging capabilities.
    /// </summary>
    public interface IRingBufferLog
    {
        void StartLogging();
        void StopLogging();
        void ClearLog();
        int LogLength { get; }
        byte[] PeekLog();
        byte[] PeekLog(int Length);
    }
}
