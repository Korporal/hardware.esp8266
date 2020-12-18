using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steadsoft.ESP8266
{
    [Flags]
    public enum WiFiStandard : byte
    {
        NONE = 0,
        S802_11b = 1,
        S802_11g = 2,
        S802_11n = 4
    }
}
