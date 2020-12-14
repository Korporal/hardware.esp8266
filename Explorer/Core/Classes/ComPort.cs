using Steadsoft.IO;
using System;
using System.IO.Ports;
using System.Threading;

namespace Steadsoft.ESP8266
{
    /// <summary>
    /// 
    /// </summary>
    public class ComPort : IRingBufferLog, IAsyncIO
    {
        private volatile bool busy; // helps handle the ESP8266's 'busy p...' and 'busy s...' responses.
        private readonly char quoteChar;
        private readonly SerialPort port;
        private readonly RingBuffer receiveBuffer;
        public ComPortSettings Settings { get; private set; }

        public int LogLength => throw new NotImplementedException();

        public delegate void ComPortReceiveEventHandler(object Sender, ComPortReceiveEventArgs Args);
        public event ComPortReceiveEventHandler DataReceived = delegate { };
        public ComPort(string PortName, ComPortSettings Settings)
        {
            if (String.IsNullOrWhiteSpace(PortName)) throw new ArgumentException("The name must have a value.", nameof(PortName));
            if (Settings == null) throw new ArgumentNullException(nameof(Settings));

            if (Settings.ReceiveBufferCapacity <= 0) throw new ArgumentException(nameof(Settings.ReceiveBufferCapacity), $"The supplied buffer capacity ({Settings.ReceiveBufferCapacity}) must be greater than zero.");
            if (Settings.BusyWaitPeriod <= 0) throw new ArgumentException(nameof(Settings.BusyWaitPeriod), $"The supplied busy wait period ({Settings.BusyWaitPeriod}) must be greater than zero.");
            if (Settings.QuoteChar != null && Settings.QuoteChar.Length > 1) throw new ArgumentException(nameof(Settings.QuoteChar), $"The supplied quote character ({Settings.QuoteChar}) must be either null or a single character.");

            port = new SerialPort
            {
                PortName = PortName,
                BaudRate = Settings.BaudRate,
                Parity = Settings.Parity,
                DataBits = Settings.DataBits,
                StopBits = Settings.StopBits,
                Handshake = Settings.FlowControl
            };
            this.Settings = Settings;
            receiveBuffer = new RingBuffer(Settings.ReceiveBufferCapacity);
            this.quoteChar = Settings.QuoteChar[0];
        }

        public void BeginReceiving()
        {
            // We simply pass this object (which implements IASyncIO) into the ring buffer's begin
            // receive method and it handles all of the buffering and callback handling for us.
            // The our own receive callback is called by the ring buffer after it has inserted
            // the received data into the ring buffer.

            var r = receiveBuffer.BeginFastRecv(this, ReceiveCallback, "hello");
        }

        private void ReceiveCallback (IFastAsyncResult Result)
        {
            bool stop = false;

            try
            {
                var args = new ComPortReceiveEventArgs(this, Result.Buffer, busy);

                DataReceived(this, args);

                busy = args.Busy;
                stop = args.Stop;
            }
            catch (Exception)
            {
                ;
            }
            finally
            {
                if (!stop)
                   BeginReceiving();
            }
        }
        public void Send(string Text)
        {
            if (Settings.QuoteChar != null)
               Text = Text.Replace(quoteChar, '"');

            while (busy) Thread.Sleep(Settings.BusyWaitPeriod);
            busy = true;
            port.Write(Text);

            if (Settings.PostWriteDelay > 0)
                Thread.Sleep(Settings.PostWriteDelay);
        }
        internal void SendLine(string Text)
        {
            Send(Text + '\r' + '\n');
        }
        public void Open()
        {
            port.Open();
        }
        public void Close()
        {
            port.Close();
        }

        public void StartLogging()
        {
            receiveBuffer.StartLogging();
        }

        public void StopLogging()
        {
            receiveBuffer.StopLogging();
        }

        public void ClearLog()
        {
            receiveBuffer.ClearLog();
        }

        public byte[] PeekLog()
        {
            return receiveBuffer.PeekLog();
        }

        public byte[] PeekLog(int Length)
        {
            return receiveBuffer.PeekLog(Length);
        }
        #region Async IO Callbacks
        IAsyncResult IAsyncIO.BeginRecv(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return port.BaseStream.BeginRead(buffer, offset, count, callback, state);
        }

        int IAsyncIO.EndRecv(IAsyncResult Result)
        {
            return port.BaseStream.EndRead(Result);
        }

        IAsyncResult IAsyncIO.BeginSend(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return port.BaseStream.BeginWrite(buffer, offset, count, callback, state);
        }

        void IAsyncIO.EndSend(IAsyncResult Result)
        {
            port.BaseStream.EndWrite(Result);
        }
        #endregion
    }
}