﻿using System;
using System.Collections;
using System.Linq;

namespace Steadsoft.Devices.WiFi.ESP8266
{
    public class ResponseLine
    {
        public static string[] CopyResponses(ArrayList Source)
        {
            if (Source == null) throw new ArgumentNullException(nameof(Source));

            string[] responses = new string[Source.Count];
            Source.CopyTo(responses);
            return responses.Select(text => text.Trim('\r', '\n')).ToArray(); ;
        }
    }
}
