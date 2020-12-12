using System;
using static Steadsoft.Devices.WiFi.ESP8266.ResponseStrings;

namespace Steadsoft.Devices.WiFi.ESP8266
{
    public sealed class TcpIp
    {
        private ESP8266 device;
        internal TcpIp(ESP8266 Device)
        {
            device = Device;
        }

        public string[] ConnectSocket(string IPAddress, int Port, int LinkID = 0)
        {
            if (LinkID < 0 || LinkID > 4)
                throw new ArgumentOutOfRangeException("The supplied link ID must be between 0 (the default) and 4 inclusive.");

            IPAddress = IPAddress ?? throw new ArgumentNullException(nameof(IPAddress));

            System.Net.IPAddress.Parse(IPAddress); // validate the address basically

            if (Port <= 0)
                throw new ArgumentException("The value must be greater than zero.", nameof(Port));
            try
            {
                string command;

                if (LinkID == 0)
                    command = $"{AT.TcpIpCommands.SOCKET_CONNECT}|TCP|,|{IPAddress}|,{Port.ToString()}";
                else
                    command = $"{AT.TcpIpCommands.SOCKET_CONNECT}{LinkID},|TCP|,|{IPAddress}|,{Port.ToString()}";
                device.receive_buffers[LinkID].Clear();
                device.Execute(command, OK);
                return ResponseLine.CopyResponses(device.results);
            }
            finally
            {
                device.results.Clear();
            }

        }

        public string[] DisconnectSocket(int LinkID = -1)
        {
            if (LinkID < 0 || LinkID > 4)
                throw new ArgumentOutOfRangeException("The supplied link ID must be between 0 (the default) and 4 inclusive.");

            try
            {
                string command;

                if (LinkID == -1)
                    command = $"{AT.TcpIpCommands.SOCKET_DISCONNECT}";
                else
                    command = $"{AT.TcpIpCommands.SOCKET_DISCONNECT}={LinkID}";

                device.Execute(command, OK);
                return ResponseLine.CopyResponses(device.results);
            }
            finally
            {
                device.results.Clear();
            }
        }

        public string[] SetConnectMode(SocketConnectMode Mode)
        {
            try
            {
                device.Execute($"{AT.TcpIpCommands.SET_SOCKET_CONNECT_MODE}{(int)Mode}", OK);
                return ResponseLine.CopyResponses(device.results);
            }
            finally
            {
                device.results.Clear();
            }
        }
    }
}