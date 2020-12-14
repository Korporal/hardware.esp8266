using System;
using static Steadsoft.ESP8266.Constants;

namespace Steadsoft.ESP8266
{
    /// <summary>
    /// Represents an ESP8266 'IPD' data packet.
    /// </summary>
    /// <remarks>
    /// IP data sent over the WiFi network via an ESP8266 consists of simple messages that contain
    /// the 'IPD' sentinel characters and a length. This information is extracted and used to create
    /// a more conveninent representation for subsequent processing by the application.
    /// </remarks>
    public class Packet
    {
        public int LinkID { get; private set; } // -1 means no link id - system is in single connection mode.
        public int Length { get; private set; }
        public byte[] Data { get; private set; }
        public Packet(int link, int length, byte[] data)
        {
            LinkID = link;
            Length = length;
            Data = data;
        }

        public static Packet Create(string Message, byte[] data)
        {
            var parts = Message.Split(Chars.COMMA, Chars.COLON);

            if (parts.Length != 3 && parts.Length != 4)
                throw new ArgumentException("The packet data does not have expected structure");

            if (parts[0] != "+IPD")
                throw new ArgumentException($"The packet's sentinel is invalid: {parts[0]}");

            if (parts.Length == 3)
            {

                if (!int.TryParse(parts[1], out int len))
                    throw new ArgumentException($"The packet's length prefix is invalid: {parts[1]}");

                if (parts[2].Length == 0)
                    throw new ArgumentException("The packet's data block must not be empty.");

                int excess = parts[0].Length + parts[1].Length + 2; // i.e. the +IPD,nnn: 

                if (data.Length - excess != len)
                    throw new InvalidOperationException("The packet's length prefix does not match the packet's content length.");

                byte[] content = new byte[len];

                Array.Copy(data, excess, content, 0, len);

                return new Packet(0, len, content);

            }
            else
            {
                if (!int.TryParse(parts[1], out int link))
                    throw new ArgumentException($"The packet's link prefix is invalid: {parts[1]}");

                if (!int.TryParse(parts[2], out int len))
                    throw new ArgumentException($"The packet's length prefix is invalid: {parts[2]}");

                if (parts[3].Length == 0)
                    throw new ArgumentException("The packet's data block must not be empty.");

                int excess = parts[0].Length + parts[1].Length + parts[2].Length + 3; // i.e. the +IPD,n,nnn: 

                if (data.Length - excess != len)
                    throw new InvalidOperationException("The packet's length prefix does not match the packet's content length.");

                byte[] content = new byte[len];

                Array.Copy(data, excess, content, 0, len);

                return new Packet(link, len, content);

            }

        }
    }
}
