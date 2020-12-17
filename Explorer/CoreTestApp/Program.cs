using Steadsoft.ESP8266;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace WiFiESP8266Testing
{
    class Program
    {
        private static ESP8266 device; // This is static simply because the Main is static, this is after all just a basic test app.
        private static bool connected = false;

        static void Main(string[] args)
        {
            try
            {
                // we expect COM port, network name to connect to and password.
                // e.g.
                // COM5 MyHomeNetwork MyPassword
                // No quotes, just three text strings separated by spaces.

                if (args.Length != 3)
                    throw new ArgumentException("Command line args are messed up.");

                ComPortSettings settings = new ComPortSettings()
                {
                    BaudRate = 115200,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    FlowControl = Handshake.None,
                    ReceiveBufferCapacity = 4096,
                    QuoteChar = "|",
                    BusyWaitPeriod = 1,
                    PostWriteDelay = 1
                };

                ComPort port = new ComPort(args[0], settings);

                device = new ESP8266(port);

                device.ConnectionChanged += OnConnectionChanged;
                device.SocketReceive += OnSocketReceive;

                device.Start();

                device.Basic.FactoryReset();

                device.Basic.DisableEcho();

                var res = device.Basic.Restart();


                var uartc = device.Basic.GetCurrentUARTConfig();
                var uartd = device.Basic.GetDefaultUARTConfig();
                var powvdd = device.Basic.GetVDDRFPower();

                var ram = device.Basic.GetFreeRam();

                var info = device.Basic.GetVersionInfo();

                device.Basic.DisableEcho();

                device.Basic.SetSleepMode(SleepMode.Disabled);

                device.WiFi.SetWiFiMode(WiFiMode.Station);

                var stn = device.WiFi.GetStationName();

                var set = device.WiFi.SetStationName("TheTardis");

                var points = device.WiFi.GetAccessPoints(AccessPointOptions.AllOptions, true).OrderByDescending(point => point.SignalStrength);

loop:
                
                for (int X = 0; X < 100; X++)
                {
                    try
                    {
                        device.WiFi.ConnectToNetwork(args[1], args[2]);
                        connected = true;
                        break;
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }
                }

                var time = device.TcpIp.GetSNTPTime();

                var status = device.TcpIp.GetConnectionStatus();

                device.TcpIp.SetConnectMode(SocketConnectMode.MultipleConnections);

                // Connect to a remote server, that server will just send data endlessly which
                // the OnSocketReceive handler below will see.

                var scr1 = device.TcpIp.ConnectSocket(SocketType.UDP, "192.168.0.19", 4567, 1);
                var scr2 = device.TcpIp.ConnectSocket(SocketType.TCP, "192.168.0.19", 4567, 2);
                var scr3 = device.TcpIp.ConnectSocket(SocketType.UDP, "192.168.0.19", 4567, 3);
                var scr4 = device.TcpIp.ConnectSocket(SocketType.TCP, "192.168.0.19", 4567, 4);

                // Once connected go to sleep, all inbound IP traffic is asynchronoulsy processed and fed to the event handler
                // This is all ultimetly done by the COM port's async callback.

                Thread.Sleep(5000);

                device.WiFi.DisconnectFromNetwork();

                goto loop;

            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }

            Thread.Sleep(-1);
        }
        private static void OnSocketReceive(object Sender, SocketReceiveEventArgs Args)
        {
            Debug.WriteLine(Args.Packet.Length);
        }
        private static void OnConnectionChanged(object Sender, WiFiConnectEventArgs Args)
        {
            Debug.WriteLine(Args.Status);
        }
    }
}