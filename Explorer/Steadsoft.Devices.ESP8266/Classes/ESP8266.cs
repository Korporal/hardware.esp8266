using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using static Steadsoft.Devices.WiFi.ESP8266.Constants;
using static Steadsoft.Devices.WiFi.ESP8266.Constants.Chars;
using static Steadsoft.Devices.WiFi.ESP8266.ResponseStrings;

namespace Steadsoft.Devices.WiFi.ESP8266
{
    /// <summary>
    /// Represents an ESP8266 WiFi microchip originally manufactured by Espressif Systems.
    /// </summary>
    /// <remarks>
    /// This class abstracts the device and exposes numerous AT commands as methods. The class
    /// inherently relies upon serial async communications with the device over a .Net com port.
    /// Most of the methods are synchronous and follow a common pattern that is to initiate IO
    /// by sending an AT command (more or less a short ASCII string sometimes with embedded arguments) 
    /// and then waiting on an event. 
    /// The event is eventually signalled by an asynchronous callback that accumulates all 
    /// received data in a ring buffer. After a block of bytes has been appended to the ring
    /// buffer within the callback, the callback attempts to find a byte sequence that 
    /// ends in CRLF and performs processing based upon that sequence. The vast majority of
    /// AT commands result in a series of response characters/strings being received ultimately followed
    /// by the sequence "OK\r\n", this is the trigger (within the callback) that is used to
    /// trigger the event and thus awaken the suspended calling user thread. These response
    /// characters/strings are accumulated in a response array (as strings) within the callback
    /// and retrieved (and possibly converted) after the event wait, these response values
    /// ultimately comprise the methods returned result(s). AT commands that return no result
    /// are implemented as void methods (e.g ATE0 or ATE1 which simply disables/enables echo).
    /// </remarks>
    public sealed class ESP8266 : IRingBufferLog
    {
        public Basic Basic { get; private set; } 
        public TcpIp TcpIp { get; private set; }
        public WiFi WiFi { get; private set; }
        public delegate void WiFiConnectionEventHandler(object Sender, WiFiConnectEventArgs Args);
        public delegate void SocketReceiveEventHandler(object Sender, SocketReceiveEventArgs Args);
        public event WiFiConnectionEventHandler ConnectionChanged = delegate { };
        public event SocketReceiveEventHandler SocketReceive = delegate { };
        private long invocationCount = 0;
        private AutoResetEvent callCompleted = new AutoResetEvent(false);
        private string sentinel = ResponseStrings.OK;

        internal RingBuffer[] receive_buffers = { new RingBuffer(4096), new RingBuffer(4096), new RingBuffer(4096), new RingBuffer(4096) };

        internal ArrayList results = new ArrayList();
        internal ComPort Port { get; private set; }

        public ESP8266(ComPort Port)
        {
            this.Port = Port ?? throw new ArgumentNullException(nameof(Port));
            Basic = new Basic(this);
            WiFi = new WiFi(this);
            TcpIp = new TcpIp(this);
        }
        public bool WiFiConnected { get; private set; }

        public int LogLength => Port.LogLength;

        public void Start()
        {
            Port.DataReceived += OnDataReceived;
            Port.Open();
            Port.BeginReceiving();
        }

        internal void Execute(string Command, string Sentinel)
        {
            try
            {
                sentinel = Sentinel;
                results.Clear();
                Port.SendLine(Command);

                if (!callCompleted.WaitOne(-1))
                    throw new TimeoutException("The response did not complete within the alloted time.");

                if (results.Count == 1 && results[0] is InvalidOperationException)
                {
                    InvalidOperationException exception = (InvalidOperationException)(results[0]);
                    throw exception;
                }
            }
            finally
            {
                callCompleted.Reset(); // Reset it because of no thread was waiting then it wont wait until we reset it
            }
        }

        /// <summary>
        /// Examines the ring buffer to ascertain if there is anything recognizable that can be processed.
        /// </summary>
        /// <remarks>
        /// The ESP8266 device has numerous idiosyncracies some are clear from the documentation whereas others are
        /// visible when using the device. All received data is written to the COM port's receive buffer (a ring buffer).
        /// This includes command responses as well as received TCP data sent by a connected sending system. Command responses
        /// are examined based on the specific nature of the response text and terminating strings or characters. 
        /// Received IP data (that is pure socket based received data sent from a connected sender) is extracted and written
        /// to a ring buffer dedicated to that connected socket. The app itself uses a handler to processes received byte blocks
        /// that are sent over TCP IP.
        /// Most commands result in a series of string results (each terminated by CRLF) which is eventually terminated by "OK" and CRLF.
        /// The response processing is therefore to basically extract each CRLF text line and create an array of strings, this process itself
        /// ends when the "OK" sentinel has been encountered.
        /// Failed commands do not return "OK" but usually "FAIL" or "ERROR" and some commands result in "ready" being sent back.
        /// The processing is therefore broken down on this basis.
        /// </remarks>
        /// <param name="Sender"></param>
        /// <param name="Args"></param>
        private void OnDataReceived(object Sender, ComPortReceiveEventArgs Args)
        {
            bool has_message = false;
            bool has_pattern = false;
            int message_length = 0;
            int pattern_length = 0;
            byte[] matching_bytes = null; ;

            invocationCount++;

            // See if the buffer currently contains the chars CR LF
            // If not just exit, but if it does we remove the entire 
            // block of bytes that ends in CR LF and we then analyze 
            // that block as a string. If all we find is just CR LF
            // then just remove these bytes and repeat.

            has_message = TryFindMessage(out message_length, Args.Buffer, IPD);

            if (has_message == false)
                has_pattern = Args.Buffer.TryFindBytes(out pattern_length, CRLF);

            while (has_message || has_pattern)
            {
                if (has_pattern && pattern_length == 2)
                {
                    has_pattern = false;
                    has_message = false;

                    Args.Buffer.Read(2);
                    has_message = TryFindMessage(out message_length, Args.Buffer, IPD);

                    if (has_message == false)
                        has_pattern = Args.Buffer.TryFindBytes(out pattern_length, CRLF);

                    continue;
                }

                // OK we have series of three or more bytes that ends in CR LF

                if (has_message)
                    matching_bytes = Args.Buffer.Read(message_length);
                else
                    matching_bytes = Args.Buffer.Read(pattern_length);

                var matching_string = Encoding.Default.GetString(matching_bytes);

                var sentinel = ExtractSentinel(matching_string);

                if (sentinel != ResponseSentinel.NONE)
                    Debug.WriteLine($"GOT: {sentinel}");

                switch (sentinel)
                {
                    case ResponseSentinel.OK:
                        {
                            if (this.sentinel != OK)
                                break;

                            Args.Busy = false;
                            callCompleted.Set(); // wake if thread is waiting.
                            break;
                        }
                    case ResponseSentinel.WIFI_CONNECTED:
                        {
                            var args = new WiFiConnectEventArgs(WiFiStatus.Connected);
                            WiFiConnected = true;
                            ConnectionChanged(this, args);
                            break;
                        }
                    case ResponseSentinel.WIFI_DISCONNECT:
                        {
                            var args = new WiFiConnectEventArgs(WiFiStatus.Disconnected);
                            WiFiConnected = false;
                            ConnectionChanged(this, args);
                            break;
                        }
                    case ResponseSentinel.WIFI_GOT_IP:
                        {
                            break;
                        }
                    case ResponseSentinel.READY:
                        {
                            if (this.sentinel != READY)
                                break;

                            Args.Busy = false;
                            callCompleted.Set(); // wake if thread is waiting.
                            break;
                        }
                    case ResponseSentinel.ERROR:
                    case ResponseSentinel.FAIL:
                        {
                            results.Add($"The device returned '{sentinel}' while processing the request for content: {matching_string}");
                            Args.Busy = false;
                            callCompleted.Set(); // wake if thread is waiting.
                            break;
                        }
                    case ResponseSentinel.NONE:
                        {
                            results.Add(matching_string);
                            break;
                        }
                    case ResponseSentinel.IPD:
                        {
                            Packet p = Packet.Create(matching_string, matching_bytes);

                            if (SocketReceive.GetInvocationList().Length == 0)
                                receive_buffers[p.LinkID].Write(p.Data);
                            else
                                SocketReceive(this, new SocketReceiveEventArgs() { LinkID = p.LinkID, Length = p.Data.Length, Buffer = p.Data });
                            Debug.WriteLine($"Received IP Data: [{matching_string.TrimEnd('\n').TrimEnd('\r')}] current ring buffer length: [{receive_buffers[p.LinkID].UsedBytes}] ({receive_buffers[p.LinkID].FreeBytes} bytes free, {receive_buffers[p.LinkID].PeakLength} peak).");
                            break;
                        }
                    case ResponseSentinel.CLOSED:
                    case ResponseSentinel.NO_IP:
                        {
                            break; // Do nothing - this seems to always follow an "ERROR" on a failed attempted socket connect
                        }
                }

                has_message = TryFindMessage(out message_length, Args.Buffer, IPD);

                if (has_message == false)
                    has_pattern = Args.Buffer.TryFindBytes(out pattern_length, CRLF);

            }
        }
        public void StartLogging()
        {
            Port.StartLogging();
        }
        public void StopLogging()
        {
            Port.StopLogging();
        }
        private static ResponseSentinel ExtractSentinel(string Data)
        {
            if (Data.Contains(ResponseStrings.READY)) return ResponseSentinel.READY;
            if (Data.Contains(ResponseStrings.ERROR)) return ResponseSentinel.ERROR;
            if (Data.Contains(ResponseStrings.FAIL)) return ResponseSentinel.FAIL;
            if (Data.Contains(ResponseStrings.CLOSED)) return ResponseSentinel.CLOSED;
            if (Data.Contains(ResponseStrings.NO_IP)) return ResponseSentinel.NO_IP;
            if (Data.Contains(ResponseStrings.WIFI_DISCONNECT)) return ResponseSentinel.WIFI_DISCONNECT;
            if (Data.Contains(ResponseStrings.WIFI_CONNECTED)) return ResponseSentinel.WIFI_CONNECTED;
            if (Data.Contains(ResponseStrings.SOCKET_CONNECTED)) return ResponseSentinel.CONNECT;
            if (Data.Contains(ResponseStrings.NETWORK_RECEIPT)) return ResponseSentinel.IPD;
            if (Data.Contains(ResponseStrings.WIFI_GOT_IP)) return ResponseSentinel.WIFI_GOT_IP;
            if (Data.Contains(ResponseStrings.OK)) return ResponseSentinel.OK;

            return ResponseSentinel.NONE;
        }

        public void ClearLog()
        {
            Port.ClearLog();
        }

        public byte[] PeekLog()
        {
            return Port.PeekLog();
        }

        public byte[] PeekLog(int Length)
        {
            return Port.PeekLog(Length);
        }

        public bool TryFindMessage(out int MatchedLength, RingBuffer RingBuffer, byte[] Bytes)
        {
                MatchedLength = 0;

                if (RingBuffer.UsedBytes <= 8) // i.e +IPD,n:X -> min size of a true message
                    return false;

                if (RingBuffer.BeginsWith(IPD))
                {
                    if (RingBuffer.TryFindBytes(out var ml, COLON))
                    {
                        var msg = RingBuffer.Peek(ml);

                        var text = Encoding.Default.GetString(msg);

                        var parts = text.Split(',', ':');

                        if (parts.Length == 3)
                        {
                            var datalen = Convert.ToInt32(parts[1]);

                            MatchedLength = datalen + msg.Length;

                            if (MatchedLength <= RingBuffer.UsedBytes)
                                return true;
                            else
                            {
                                MatchedLength = 0;
                                return false;
                            }

                        }
                        else
                        {
                            var datalen = Convert.ToInt32(parts[2]);

                            MatchedLength = datalen + msg.Length;

                            if (MatchedLength <= RingBuffer.UsedBytes)
                                return true;
                            else
                            {
                                MatchedLength = 0;
                                return false;
                            }
                        }
                    }
                }

                return false;
        }
    }
}
