using System;

namespace Steadsoft.Devices.WiFi.ESP8266
{
    [Flags]
    public enum AccessPointOptions
    {
        Encryption = 1,
        SSID = 2 * Encryption,
        SignalStrength = 2 * SSID,
        MAC = 2 * SignalStrength,
        Channel = 2 * MAC,
        FrequencyOffset = 2 * Channel,
        FrequencyCalibration = 2 * FrequencyOffset,
        PairwiseChiper = 2 * FrequencyCalibration,
        GroupChipher = 2 * PairwiseChiper,
        Standards = 2 * GroupChipher,
        WPS = 2 * Standards,
        AllOptions = Encryption | SSID | SignalStrength | MAC | Channel | FrequencyOffset | FrequencyCalibration | PairwiseChiper | GroupChipher | Standards | WPS
    }
}
