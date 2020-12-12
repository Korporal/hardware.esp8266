using System.IO.Ports;

namespace Steadsoft.Devices.WiFi.ESP8266
{
    public class ComPortSettings
    {
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake FlowControl { get; set; }
        public int ReceiveBufferCapacity { get; set; }
        /// <summary>
        /// A character that if present in a sent message is replaced with a quote.
        /// </summary>
        /// <remarks>
        /// This is an option to make user code easier to read and allows a special
        /// character to be used in sent messages that represents a quote. The character
        /// is then replaced with a true quote prior to transmission.
        /// </remarks>
        public string QuoteChar { get; set; }
        /// <summary>
        /// The polling interval used when sending and a prior request has not yet completed.
        /// </summary>
        public int BusyWaitPeriod { get; set; }
        /// <summary>
        /// Causes the calling thread to sleep for a period after sending data.
        /// </summary>
        /// <remarks>
        /// This is used to sleep the sender thread after a send has been performed. This 
        /// allows other threads in the app to run in cases where async IO may be taking
        /// place. This is most useful when debugging because a simple breakpoint
        /// when hit freezes all threads and makes it hard to monitor IO. This allows
        /// the thread to sleep before hiitng a breakpoint and thus avoid the situation
        /// where all threads freeze prematurely impacting async receive.
        /// </remarks>
        public int PostWriteDelay { get; set; }
    }
}
