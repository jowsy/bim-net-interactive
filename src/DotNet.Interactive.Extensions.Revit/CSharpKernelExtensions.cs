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
                                using (var pipe = new NamedPipeClientStream("localhost", $"revitdispatcher{revitVersion}", PipeDirection.InOut))
                                {
                                    pipe.Connect(10000);
                                    pipe.ReadMode = PipeTransmissionMode.Message;

                                    var input = submitCode.Code;
                                    byte[] bytes = Encoding.Default.GetBytes(input);
                                    pipe.Write(bytes, 0, bytes.Length);
                                    var result = ReadMessage(pipe);
                                    context.DisplayStandardOut(Encoding.UTF8.GetString(result));
                                  
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
