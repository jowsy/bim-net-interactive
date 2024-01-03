using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive.Connection;
using Microsoft.DotNet.Interactive;
using Jo.Interactive.Extensions.Revit;
using Microsoft.DotNet.Interactive.Commands;

namespace DispatcherConsole
{
    internal class Program
    {

        private const string NamedPipeName = "revit-kernel-2014-pipe";
        private static CompositeKernel _kernel;

        static void Main(string[] args)
        {
            _kernel = new CompositeKernel();

            var revitKernel = new RevitKernel("RevitCSharpKernel");

            _kernel.Add(revitKernel);
            revitKernel.UseValueSharing();
       
            revitKernel.AddMiddleware(async (KernelCommand command, KernelInvocationContext context, KernelPipelineContinuation next) =>
            {
                /*if (RunOnDispatcher)
                {
                    await Dispatcher.InvokeAsync(async () => await next(command, context));
                }
                else
                {*/
                    await next(command, context);
                //}
            });
            SetUpNamedPipeKernelConnection();
            /* while (true)
             {
                 using (var pipe = new NamedPipeServerStream(
                 "revit-kernel-2014-pipe",
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

             }*/

            /*  var _ = Task.Run(async () =>
              {
                  //Load WPF app assembly 
                  await csharpKernel.SendAsync(new SubmitCode(@$"#r ""{typeof(App).Assembly.Location}""
  using {nameof(WpfConnect)};"));
                  //Add the WPF app as a variable that can be accessed
                  await csharpKernel.SetValueAsync("App", this, GetType());
                  
                  //Start named pipe
                  _kernel.AddKernelConnector(new ConnectNamedPipeCommand());
              });*/

            Console.ReadKey();
        }

        private static void SetUpNamedPipeKernelConnection()
        {
            var serverStream = new NamedPipeServerStream(
                NamedPipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous);

            var sender = KernelCommandAndEventSender.FromNamedPipe(
                serverStream,
                new Uri("kernel://remote-control"));
            var receiver = KernelCommandAndEventReceiver.FromNamedPipe(serverStream);

            var host = _kernel.UseHost(sender, receiver, new Uri("kernel://my-wpf-app"));

            _kernel.RegisterForDisposal(host);
            _kernel.RegisterForDisposal(receiver);
            _kernel.RegisterForDisposal(serverStream);

            var _ = Task.Run(() =>
            {
                // required as waiting connection on named pipe server will block
                serverStream.WaitForConnection();
                var _host = host.ConnectAsync();
            });
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
