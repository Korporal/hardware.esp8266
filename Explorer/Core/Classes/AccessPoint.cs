using System;
using System.Collections;
using static Steadsoft.ESP8266.Constants;
using static Steadsoft.ESP8266.ResultPrefix;

namespace Steadsoft.ESP8266
{
    public sealed class AccessPoint : ResponseLine
    {
        private AccessPoint (AccessPointOptions Options, bool RSSIOrdering, string Description)
        {
            // +CWLAP:(0,"FaryLink_24334A",-12,"be:dd:c2:24:33:4a",1,20,0)

            if (!Description.StartsWith(CWLAP))
                throw new ArgumentException($"The {nameof(Description)} must start with '{CWLAP}'.");

            Description = Description.Replace(@"""","");

            int lpar = Description.IndexOf(Chars.LPAR);
            int rpar = Description.IndexOf(Chars.RPAR);

            string raw = Description[(lpar + 1)..rpar];

            var parts = raw.Split(Chars.COMMA);

            int sub = 0;

            this.Options = Options;

            if (Options.HasFlag(AccessPointOptions.Encryption))
                Encryption = (AccessPointEncryption)Convert.ToInt32(parts[sub++]);

            if (Options.HasFlag(AccessPointOptions.SSID))
                SSID = parts[sub++];

            if (Options.HasFlag(AccessPointOptions.SignalStrength))
                SignalStrength = Convert.ToInt32(parts[sub++]);

            if (Options.HasFlag(AccessPointOptions.MAC))
                MAC = parts[sub++];

            if (Options.HasFlag(AccessPointOptions.Channel))
                Channel = Convert.ToInt32(parts[sub++]);

            if (Options.HasFlag(AccessPointOptions.FrequencyOffset))
                FrequencyOffset = Convert.ToInt32(parts[sub++]);

            if (Options.HasFlag(AccessPointOptions.FrequencyCalibration))
                FrequencyCalibration = Convert.ToInt32(parts[sub++]);

            // other options never resulted in us seeing their values returned...
        }

        /// <summary>
        /// Indicates which of the properties have a meaningful value.
        /// </summary>
        public AccessPointOptions Options { get; private set; }
        public AccessPointEncryption Encryption { get; private set; }
        public string SSID { get; private set; }
        public int SignalStrength { get; private set; }
        public string MAC { get; private set; }
        public int Channel { get; private set; }
        public int FrequencyOffset { get; private set; }
        public int FrequencyCalibration { get; private set; }

        internal static AccessPoint[] CreateFromSource(AccessPointOptions Options, bool RSSIOrdering, ArrayList Source)
        {
            if (Source == null) throw new ArgumentNullException(nameof(Source));

            string[] lines = CopyResponses(Source);

            AccessPoint[] points = new AccessPoint[lines.Length];

            for (int I = 0; I < points.Length; I++)
            {
                points[I] = new AccessPoint(Options, RSSIOrdering, lines[I]);
            }

            return points;
        }

        public override string ToString()
        {
            return $"{SSID} ({Encryption})";
        }
    }

}
