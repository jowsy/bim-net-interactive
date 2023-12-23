using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                using (var pipe = new NamedPipeServerStream(
                "revitdispatcher2024",
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Message))
                {

                    Console.WriteLine("[*] Waiting for client connection...");
                    pipe.WaitForConnection();
                    Console.WriteLine("[*] Client connected.");

                    var messageBytes = ReadMessage(pipe);
                    var line = Encoding.UTF8.GetString(messageBytes);
                    Console.WriteLine("[*] Received: {0}", line);
                    if (line.ToLower() == "exit") return;
                    var response = Encoding.UTF8.GetBytes("Hello on the other side !");
                    pipe.Write(response, 0, response.Length);

                }

            }
        
            Console.ReadKey();
        }

        private static byte[] ReadMessage(PipeStream pipe)
        {
            byte[] buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                do
                {
                    var readBytes = pipe.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, readBytes);
                }
                while (!pipe.IsMessageComplete);

                return ms.ToArray();
            }
        }
    }
}
