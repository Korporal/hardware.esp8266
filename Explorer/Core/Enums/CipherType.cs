using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steadsoft.ESP8266
{
    public enum CipherType
    {
        NONE,
        WEP40,
        WEP104,
        TKIP,
        CCMP,
        TKIP_CCMP,
        UNKNOWN
    }
}
