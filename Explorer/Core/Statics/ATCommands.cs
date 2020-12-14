namespace Steadsoft.ESP8266
{
    internal static class AT
    {
        internal static class BasicCommands
        {
            internal const string ATE0 = "ATE0";
            internal const string ATE1 = "ATE1";
            internal const string RESTART = "AT+RST";
            internal const string RESTORE = "AT+RESTORE";
            internal const string UPDATE_FIRMWARE = "AT+CIUPDATE";
            internal const string GET_VERSION_INFO = "AT+GMR";
            internal const string GET_FREE_RAM = "AT+SYSRAM?";
            internal const string GET_ADC_VALUE = "AT+SYSADC?";
            internal const string SET_SLEEP_MODE = "AT+SLEEP=";
            internal const string GET_VDD_RF_POWER = "AT+RFVDD?";
            /// <summary>
            /// Defines commands that result in the device's flash memory being updated.
            /// </summary>
            internal static class FLASH
            {
                internal const string GET_DEF_UART_CONFIG = "AT+UART_DEF?";
            }
            /// <summary>
            /// Defines commands that do not result in the device's flash memory being updated.
            /// </summary>
            internal static class NO_FLASH
            {
                internal const string GET_CURR_UART_CONFIG = "AT+UART_CUR?";
            }
        }

        internal static class WiFiCommands
        {
            internal static class NO_FLASH
            {
                internal const string SET_WIFI_MODE = "AT+CWMODE_CUR=";
                internal const string CONNECT_TO_ACCESS_POINT = "AT+CWJAP_CUR=";
            }

            internal const string GET_CURRENT_IP_INFO = "AT+CIPSTA_CUR?";
            internal const string DISCONNECT_FROM_ACCESS_POINT = "AT+CWQAP";
            internal const string SET_ACCESS_POINT_OPTIONS = "AT+CWLAPOPT=";
            internal const string LIST_ACCESS_POINTS = "AT+CWLAP";
            internal const string GET_CONNECTED_STATION_IP = "AT+CWLIF";
            internal const string UPDATE_FIRMWARE_OTA = "AT+CIUPDATE";
            internal const string GET_STATION_NAME = "AT+CWHOSTNAME?";
            internal const string SET_STATION_NAME = "AT+CWHOSTNAME=";

        }

        internal static class TcpIpCommands
        {
            internal const string GET_SOCKET_CONNECT_MODE = "AT+CIPMUX?";
            internal const string SET_SOCKET_CONNECT_MODE = "AT+CIPMUX=";
            internal const string SOCKET_CONNECT = "AT+CIPSTART=";
            internal const string SOCKET_DISCONNECT = "AT+CIPCLOSE";
            internal const string GET_SNTP_TIME = "AT+CIPSNTPTIME?";
            internal const string GET_CONNECTION_STATUS = "AT+CIPSTATUS";

        }
    }


}