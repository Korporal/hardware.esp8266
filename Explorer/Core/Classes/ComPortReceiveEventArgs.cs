using System;
using Steadsoft.IO;

namespace Steadsoft.ESP8266
{
    public class ComPortReceiveEventArgs : EventArgs
    {
        public ComPort Source { get; private set; }
        public RingBuffer Buffer { get; private set; }
        /// <summary>
        /// Used by handler to discontinue further reading.
        /// </summary>
        public bool Stop { get; set; }
        /// <summary>
        /// Used by a handler to indicate if further sending should be blocked.
        /// </summary>
        public bool Busy { get; set; }

        public ComPortReceiveEventArgs(ComPort Source, RingBuffer Buffer, bool Busy)
        {
            this.Source = Source;
            this.Buffer = Buffer;
            this.Busy = Busy;
        }
    }
}
