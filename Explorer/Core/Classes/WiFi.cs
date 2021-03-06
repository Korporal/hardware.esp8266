﻿using System;
using System.Net;
using static Steadsoft.ESP8266.Constants;
using static Steadsoft.ESP8266.ResponseStrings;
using static Steadsoft.ESP8266.ResultPrefix;

namespace Steadsoft.ESP8266
{
    public sealed class WiFi
    {
        private readonly ESP8266 device;

        internal WiFi(ESP8266 Device)
        {
            device = Device;
        }

        public string GetStationName()
        {
            try
            {
                device.Execute($"{AT.WiFiCommands.GET_STATION_NAME}", OK);
                return ResponseLine.CopyResponses(device.results)[0].Split(Chars.COLON)[1];
            }
            finally
            {
                device.results.Clear();
            }

        }
        public string[] SetStationName(string Name)
        {
            try
            {
                device.Execute($"{AT.WiFiCommands.SET_STATION_NAME}\"{Name}\"", OK);
                return ResponseLine.CopyResponses(device.results);
            }
            finally
            {
                device.results.Clear();
            }

        }

        public void SetWiFiMode(WiFiMode Mode)
        {
            try
            {
                device.Execute($"{AT.WiFiCommands.NO_FLASH.SET_WIFI_MODE}{(int)Mode}", OK);
            }
            finally
            {
                device.results.Clear();
            }
        }

        public void ConnectToNetwork(string StationID, string Password)
        {
            if (string.IsNullOrWhiteSpace(StationID)) throw new ArgumentException("The supplied value is invalid.", nameof(StationID));
            if (string.IsNullOrWhiteSpace(Password)) throw new ArgumentException("The supplied value is invalid.", nameof(Password));

            try
            {
                device.Execute($"{AT.WiFiCommands.NO_FLASH.CONNECT_TO_ACCESS_POINT}{device.Port.Settings.QuoteChar}{StationID}{device.Port.Settings.QuoteChar},{device.Port.Settings.QuoteChar}{Password}{device.Port.Settings.QuoteChar}", "OK");

                if (device.results.Count > 0)
                    if (device.results[0] != null)
                        {
                        if (device.results[0].ToString().Contains("CWJAP:1"))
                            throw new TimeoutException();
                        if (device.results[0].ToString().Contains("CWJAP:2"))
                            throw new InvalidOperationException("Bad password");
                        if (device.results[0].ToString().Contains("CWJAP:3"))
                            throw new InvalidOperationException($"Access point '{StationID}' was not found");
                        if (device.results[0].ToString().Contains("CWJAP:4"))
                            throw new InvalidOperationException("The connection failed");
                    }


                // replies like CWJAP:n are error info and n can be 1,2,3 or 4
            }
            finally
            {
                device.results.Clear();
            }
        }


        public IPAddress[] GetCurrentIPInfo()
        {
            try
            {
                device.Execute(AT.WiFiCommands.GET_CURRENT_IP_INFO, OK);
                IPAddress[] ips = new IPAddress[device.results.Count];

                for (int I = 0; I < device.results.Count; I++)
                {
                    var text = (string)(device.results[I]);

                    if (!text.StartsWith(CIPSTA_CUR)) throw new ArgumentException($"The {nameof(text)} must start with '{CIPSTA_CUR}'.");

                    var parts = text.Split(Chars.QUOTE);

                    var kind = Enum.Parse(typeof(IPAddressKind), parts[0].Split(Chars.COLON)[1], true);

                    ips[(int)kind] = IPAddress.Parse(parts[1]);
                }

                return ips;
            }
            finally
            {
                device.results.Clear();
            }
        }
        public StationInfo[] GetStationIP()
        {
            try
            {
                device.Execute(AT.WiFiCommands.GET_CONNECTED_STATION_IP, OK);

                StationInfo[] info = new StationInfo[device.results.Count];

                if (device.results.Count == 0)
                    return info;

                info[0] = StationInfo.CreateFromString((string)(device.results[0]));

                return info;
            }
            finally
            {
                device.results.Clear();
            }

        }

        public void DisconnectFromNetwork()
        {
            try
            {
                device.Execute(AT.WiFiCommands.DISCONNECT_FROM_ACCESS_POINT, OK);
            }
            finally
            {
                device.results.Clear();
            }
        }

        private void SetAccessPointOptions(AccessPointOptions Options, bool RSSIOrdering)
        {
            try
            {
                device.Execute($"{AT.WiFiCommands.SET_ACCESS_POINT_OPTIONS}{Convert.ToInt32(RSSIOrdering)},{(int)(Options)}", OK);
                return;
            }
            finally
            {
                device.results.Clear();
            }
        }
        public AccessPoint[] GetAccessPoints(AccessPointOptions Options, bool RSSIOrdering)
        {
            try
            {
                SetAccessPointOptions(Options, RSSIOrdering);
                device.Execute(AT.WiFiCommands.LIST_ACCESS_POINTS, OK);
                return AccessPoint.CreateFromSource(Options, RSSIOrdering, device.results);
            }
            finally
            {
                device.results.Clear();
            }
        }



    }
}
