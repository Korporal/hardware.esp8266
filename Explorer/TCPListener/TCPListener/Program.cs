using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;      //required
using System.Net.Sockets;    //required
using System.Threading;

namespace ServerTest
{
    class Program
    {
        private static Random timeRand = new Random();
        private static Random dataRand = new Random();
        static void Main(string[] args)
        {
            Console.WindowHeight = 50;
            Console.WindowWidth = 50;

            var ip = IPAddress.Parse("192.168.0.19");

            //var ip = IPAddress.Any;

            TcpListener listener = new TcpListener(ip, 4567);

            // we set our IP address as server's address, and we also set the port: 9999

            listener.Start();  // this will start the server

            Console.WriteLine("Started listening for connection requests on: " + listener.LocalEndpoint.ToString());

            while (true)   //we wait for a connection
            {
                using (TcpClient client = listener.AcceptTcpClient())                //if a connection exists, the server will accept it

                {
                    Console.WriteLine($"{Time} - connection established on: " + client.Client.LocalEndPoint.ToString());

                    client.ReceiveTimeout = 500;

                    NetworkStream ns = client.GetStream(); //networkstream is used to send/receive messages

                    byte[] hello = new byte[100];   //any message must be serialized (converted to byte array)

                    var txt = RandomString(4, 128);

                    hello = Encoding.Default.GetBytes(txt);  //conversion string => byte array

                    ns.Write(hello, 0, hello.Length);     //sending the message

                    bool zero_was_read = false;

                    while (client.Connected && !zero_was_read)  //while the client is connected, we look for incoming messages
                    {
                        try
                        {
                            Thread.Sleep(1000);

                            txt = RandomString(4, 1024);

                            hello = Encoding.Default.GetBytes(txt);  //conversion string => byte array

                            ns.Write(hello, 0, hello.Length);     //sending the message

                            Console.WriteLine("Wrote " + hello.Length + " bytes.");

                            byte[] msg = new byte[1024];     //the messages arrive as byte array

                            int l = ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client

                            if (l > 0)
                                Console.WriteLine(Encoding.Default.GetString(msg).Trim('\x00')); //now , we write the message as string
                            else
                            {
                                zero_was_read = true;
                                Console.WriteLine($"{Time} - remote end has closed the connection.");
                            }
                        }
                        catch (Exception e) when (e.Message.Contains("period of time"))
                        {
                            ; // we don't care
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error: " + e.Message);
                            client.Close();
                        }
                    }

                    Console.WriteLine($"{Time} - connection closed.");
                }
            }
        }

        public static string Time
        {
            get
            {
                return DateTime.Now.ToLongTimeString();
            }
        }

        public static string RandomString(int min, int max)
        {
            var size = dataRand.Next(min, max);
            string input = ((char) (dataRand.Next(0x41, 0x5A))).ToString();
            return new string(Enumerable.Range(0, size).Select(x => input[dataRand.Next(0, input.Length)]).ToArray());
        }
    }
}