using System;

namespace Steadsoft.ESP8266
{
    public class VersionInfo
    {
        public string AT { get; private set; }
        public string SDK { get; private set; }
        public string Built { get; private set; }
        public string BIN { get; private set; }
        private static string SEP = Environment.NewLine;

        public VersionInfo (string[] Values)
        {
            if (Values == null || Values.Length == 0)
                return;

            if (Values.Length > 0)
                if (Values[0] != null)
                    AT = Values[0];


            if (Values.Length > 1)
                if (Values[1] != null)
                    SDK = Values[1];

            if (Values.Length > 2)
                if (Values[2] != null)
                    Built = Values[2];


            if (Values.Length > 3)
                if (Values[3] != null)
                    BIN = Values[3];

        }

        public override string ToString()
        {
            return AT + SEP + SDK + SEP + Built + SEP + BIN;
        }
    }
}
