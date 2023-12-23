using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Utility;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Globalization;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DotNet.Interactive.Extensions.Revit
{
    public static class CSharpKernelExtensions
    {
        public static CSharpKernel UseRevit(this CSharpKernel kernel, string revitVersion)
        {
            
            //InteractiveHost interactiveHost = null;

            var directive = new Command($"#!revit{revitVersion}", "Run code in Autodesk Revit as an external command. Requires the Interactive Dispatcher addin.")
            {
               
                Handler = CommandHandler.Create((InvocationContext cmdLineContext) =>
                {
                    var context = cmdLineContext.BindingContext.GetService(typeof(KernelInvocationContext)) as KernelInvocationContext;
                    if (context != null)
                    {
                        if (context.Command is SubmitCode submitCode)
                        {
                            try
                            {
                                RevitConnectorService connectorService = new RevitConnectorService();
                                connectorService.Send(submitCode.Code);

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
                                    context.DisplayStandardOut(line);
                                    //if (line.ToLower() == "exit") return;
                                    //var response = Encoding.UTF8.GetBytes("Hello on the other side !");
                                    //pipe.Write(response, 0, response.Length);

                                }

                            }catch(TimeoutException)
                            {
                                context.Fail(submitCode, message: "Could not reach revit connector. Timeout.");
                            }
                            catch(Exception ex)
                            {
                                context.DisplayStandardError($"Unexpected error: {ex.ToString()}");
                            }

                            context.Complete(submitCode);

                        };
                    }
                })
            };

            kernel.AddDirective(directive);

            return kernel;

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
