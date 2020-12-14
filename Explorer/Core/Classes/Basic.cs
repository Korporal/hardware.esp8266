using System;
using static Steadsoft.Devices.WiFi.ESP8266.ResponseStrings;

namespace Steadsoft.Devices.WiFi.ESP8266
{
    public sealed class Basic
    {
        private ESP8266 device;
        internal Basic (ESP8266 Device)
        {
            device = Device;
        }

        public void SetSleepMode(SleepMode Mode)
        {
            try
            {
                device.Execute($"{AT.BasicCommands.SET_SLEEP_MODE}{(int)Mode}", OK);
            }
            finally
            {
                device.results.Clear();
            }
        }

        public VersionInfo GetVersionInfo()
        {
            try
            {
                device.Execute(AT.BasicCommands.GET_VERSION_INFO, OK);
                return new VersionInfo(ResponseLine.CopyResponses(device.results));
            }
            finally
            {
                device.results.Clear();
            }
        }


        public void EnableEcho()
        {
            try
            {
                device.Execute(AT.BasicCommands.ATE1, OK);
            }
            finally
            {
                device.results.Clear();
            }
        }

        public void DisableEcho()
        {
            try
            {
                device.Execute(AT.BasicCommands.ATE0, OK);
            }
            finally
            {
                device.results.Clear();
            }
        }

        public string[] FactoryReset()
        {
            try
            {
                device.Execute(AT.BasicCommands.RESTORE, READY);
                return ResponseLine.CopyResponses(device.results);
            }
            finally
            {
                device.results.Clear();
            }

        }

        public string[] Restart()
        {
            try
            {
                device.Execute(AT.BasicCommands.RESTART, READY);
                return ResponseLine.CopyResponses(device.results);
            }
            finally
            {
                device.results.Clear();
            }
        }

        public string[] GetFreeRam()
        {
            try
            {
                device.Execute(AT.BasicCommands.GET_FREE_RAM, OK);
                return ResponseLine.CopyResponses(device.results);
            }
            finally
            {
                device.results.Clear();
            }
        }

        public string[] GetADCValue()
        {
            try
            {
                device.Execute(AT.BasicCommands.GET_ADC_VALUE, OK);
                return ResponseLine.CopyResponses(device.results);
            }
            finally
            {
                device.results.Clear();
            }
        }

        public void UpdateFirmware()
        {
            try
            {
                //throw new NotImplementedException();
                device.Execute($"{AT.BasicCommands.UPDATE_FIRMWARE}", OK);
                return; // AccessPoint.CreateFromSource(results);
            }
            finally
            {
                device.results.Clear();
            }
        }


    }
}
