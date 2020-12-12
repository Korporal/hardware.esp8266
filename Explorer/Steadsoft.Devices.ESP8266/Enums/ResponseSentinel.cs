namespace Steadsoft.Devices.WiFi.ESP8266
{
    public enum ResponseSentinel
    {
        NONE,
        READY,
        OK,
        ERROR,
        FAIL,
        WIFI_CONNECTED,
        WIFI_GOT_IP,
        WIFI_DISCONNECT,
        CLOSED,
        NO_IP,
        IPD,
        CONNECT // Indicates the device has established a socket connection.
    }
}
