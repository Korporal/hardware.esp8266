using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;      
using System.Net.Sockets;    
using System.Threading;
using System.Threading.Tasks;

namespace ServerTest
{
    /// <summary>
    /// Waits for TCP connections to arrive and then for each of these periodically sends a random length byte block with each byte the same value.
    /// This is used to drive and stress test ESP8266 devices and supporting code, primarily the robustness of the ring buffer design.
    /// </summary>
    public class  Program
    {
        private static readonly Random dataRand = new Random();
        static async Task Main(string[] args)
        {
            Console.WindowHeight = 50;
            Console.WindowWidth = 50;

            var ip = IPAddress.Parse("192.168.0.19"); // whatever

            TcpListener listener = new TcpListener(ip, 4567); // just a random unused port number

            var tasks = new List<Task>();

            listener.Start();  

            var listener_task = listener.AcceptSocketAsync();

            var listener_task_id = listener_task.Id;

            tasks.Add(listener_task);

            while (NotEnded(listener_task))
            {
                Console.WriteLine($"Awaiting on {tasks.Count} tasks.");

                var completed_task = await Task.WhenAny(tasks);

                // A connection has been established

                if (completed_task.Id == listener_task_id)
                {
                    var socket = ((Task<Socket>)(completed_task)).Result;

                    Console.WriteLine($"{socket.ProtocolType} connection established with {socket.RemoteEndPoint}");

                    tasks.Remove(completed_task); // We're done with this listener task.

                    var generator_task = Task.Run(() =>
                    {
                        int bytes = 0;

                        while (true)
                        {

                            try
                            {
                                bytes += socket.Send(RandomByteBlock(4,256));     
                                Thread.Sleep(DateTime.Now.Millisecond); // Crude but avoids flooding with data at an uncontrolled rate.
                            }
                            catch (Exception e)
                            {
                                throw new InvalidOperationException($"{e.Message} ({socket.RemoteEndPoint}) {bytes} bytes were sent.");
                            }
                        }
                    });

                    tasks.Add(generator_task);

                    listener_task = listener.AcceptSocketAsync();

                    listener_task_id = listener_task.Id;

                    tasks.Add(listener_task);

                    continue;
                }

                // A task that was sending data has terminted...

                tasks.Remove(completed_task);

                if (completed_task.Status == TaskStatus.Faulted)
                    Console.WriteLine($"{completed_task.Exception.Message}({completed_task.Exception.InnerExceptions[0].Message})");
            }
        }

        private static bool NotEnded(Task Task)
        {
            return !(Task.IsCanceled || Task.IsFaulted || Task.IsCompleted);
        }

        private static byte[] RandomByteBlock(int min, int max)
        {
            byte[] bytes;
            var text = RandomString(4, 128);
            bytes = Encoding.Default.GetBytes(text);
            return bytes;
        }

        public static string RandomString(int min, int max)
        {
            var size = dataRand.Next(min, max);
            string input = ((char) (dataRand.Next(0x41, 0x5A))).ToString();
            return new string(Enumerable.Range(0, size).Select(x => input[dataRand.Next(0, input.Length)]).ToArray());
        }
    }
}