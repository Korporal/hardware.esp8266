namespace Steadsoft.ESP8266
{
    public class WiFiConnectEventArgs
    {
        public WiFiStatus Status { get; private set; }
        public WiFiConnectEventArgs(WiFiStatus Status)
        {
            this.Status = Status;
        }
    }
}