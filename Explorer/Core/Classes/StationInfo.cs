using System;
using System.Net;

namespace Steadsoft.ESP8266
{
    /// <summary>
    /// Describes a connected access point.
    /// </summary>
    public sealed class StationInfo
    {
        public IPAddress IPAddress { get; private set; }
        public string MAC { get; set; }

        private StationInfo(IPAddress IPAddress, string MAC)
        {
            this.IPAddress = IPAddress;
            this.MAC = MAC;
        }

        public static StationInfo CreateFromString(string Info)
        {
            if (string.IsNullOrWhiteSpace(Info)) throw new ArgumentException("The supplied argument must have a value.", nameof(Info));

            var parts = Info.Split(',');

            var ip = IPAddress.Parse(parts[0]);

            return new StationInfo(ip, parts[1].TrimEnd('\r','\n'));
        }
    }
}
