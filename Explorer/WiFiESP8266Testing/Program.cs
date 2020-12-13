using Steadsoft.Devices.WiFi.ESP8266;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Threading;

namespace WiFiESP8266Testing
{
    class Program
    {
        private static ESP8266 device;

        static void Main(string[] args)
        {
            try
            {
                //AccessPoint[] points;
                IPAddress[] ips;

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

                device = new ESP8266(port); //, 4096 * 2);

                //wifi.StartLogging();

                device.ConnectionChanged += OnConnectionChanged;
                device.SocketReceive += OnSocketReceive;

                device.Start();

                var ram = device.Basic.GetFreeRam();

                device.Basic.Restart();

                var info = device.Basic.GetVersionInfo();

                device.Basic.DisableEcho();

                device.Basic.SetSleepMode(SleepMode.Disabled);

                device.WiFi.SetWiFiMode(WiFiMode.Station);

                var points = device.WiFi.GetAccessPoints(AccessPointOptions.AllOptions, true).OrderByDescending(point => point.SignalStrength);


                for (int X=0; X < 100; X++)
                {
                    try
                    {
                        device.WiFi.ConnectToNetwork(args[1], args[2]);
                        break;
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }
                }

                device.TcpIp.SetConnectMode(SocketConnectMode.SingleConnection);

                device.TcpIp.ConnectSocket("192.168.0.19", 4567);

                Thread.Sleep(1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //var logdata = wifi.PeekLog();

            //File.WriteAllBytes("raw_log.log", logdata);

            Thread.Sleep(-1);
        }
        private static void OnSocketReceive(object Sender, SocketReceiveEventArgs Args)
        {
            Debug.WriteLine(Args.Length);
        }
        private static void OnConnectionChanged(object Sender, WiFiConnectEventArgs Args)
        {
            Debug.WriteLine(Args.Status);
        }
    }
}