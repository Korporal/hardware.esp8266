namespace Steadsoft.ESP8266
{
    public class SocketReceiveEventArgs
    {
        public int LinkID { get; internal set; }
        public int Length { get; internal set; }
        public byte[] Buffer { get; internal set; }
    }
}