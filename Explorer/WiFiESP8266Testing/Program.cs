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
        private static ESP8266 wifi;

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
                    PostWriteDelay = 0
                };

                ComPort port = new ComPort(args[0], settings);

                wifi = new ESP8266(port); //, 4096 * 2);

                wifi.StartLogging();

                wifi.ConnectionChanged += OnConnectionChanged;
                wifi.SocketReceive += OnSocketReceive;

                wifi.Start();

                //var info = wifi.Restart();

                wifi.Basic.DisableEcho();

                //var adc = wifi.Basic.GetADCValue();

                //var ram = wifi.Basic.GetFreeRam();

                ////var adc = wifi.GetADCValue();

                ////var ram = wifi.GetFreeRam();

                //var ver = wifi.Basic.GetVersionInfo();

                wifi.WiFi.SetWiFiMode(WiFiMode.Dual);

                //for (int I = 0; I < 100; I++)
                //{

                var points = wifi.WiFi.GetAccessPoints(AccessPointOptions.AllOptions, true).OrderByDescending(point => point.SignalStrength);


                wifi.WiFi.ConnectToNetwork(args[1], args[2]);

                for (int x = 0; x < 100; x++)
                {
                    wifi.Basic.UpdateFirmware(Update.FindServer);
                    Thread.Sleep(1000);
                }


                wifi.TcpIp.SetConnectMode(SocketConnectMode.SingleConnection);

                wifi.TcpIp.ConnectSocket("192.168.0.19", 4567);

                //points = wifi.WiFi.GetAccessPoints(AccessPointOptions.AllOptions, true);

                //ips = wifi.WiFi.GetCurrentIPInfo();

                //wifi.WiFi.DisconnectFromNetwork();

                Thread.Sleep(1);
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            var logdata = wifi.PeekLog();

            File.WriteAllBytes("raw_log.log", logdata);

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