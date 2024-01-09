using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Connection;
using Microsoft.DotNet.Interactive.Events;
using RevitKernel.Core;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Interactive.Extensions.Revit
{
    public class ConnectRevitKernelCommand : ConnectKernelCommand
    {
        public Option<string> RevitVersionOption { get; } =
    new("--revit-version", "version of revit to connect to")
    {
        IsRequired = true
    };

        public ConnectRevitKernelCommand() : base("revit", "connect to a revit kernel")
        {
            Add(RevitVersionOption);
        }

        public override async Task<IEnumerable<Kernel>> ConnectKernelsAsync(KernelInvocationContext context, InvocationContext commandLineContext)
        {
            var revitversion = commandLineContext.ParseResult.GetValueForOption(RevitVersionOption);

            var pipeName = $"revit-kernel-{revitversion}-pipe";


            var connector = new NamedPipeKernelConnector(pipeName!);

            var localName = commandLineContext.ParseResult.GetValueForOption(KernelNameOption);

            var proxyKernel = await connector.CreateKernelAsync(localName!);

            
            proxyKernel.AddMiddleware(async (KernelCommand command, KernelInvocationContext context, KernelPipelineContinuation next) =>
            {
                var submitCommand = command as SubmitCode;
                if (submitCommand != null)
                {
                    RoslynCompilerService roslynCompilerService = new RoslynCompilerService();
                    var assemblyPath = roslynCompilerService.CompileCode(context, submitCommand.Code);
                    await proxyKernel.SendAsync(new SendValue("assemblyPath", assemblyPath));
                    if (assemblyPath == null)
                    {
                        context.Fail(command);
                    }
                   
                    await next(command, context);
                }
                else
                {
                    await next(command, context);
                }
            

              
                
            });
            proxyKernel.UseValueSharing();

            //Add support for sendvalue
            proxyKernel.KernelInfo.SupportedKernelCommands.Add(new KernelCommandInfo("SendValue"));
            
            return new ProxyKernel[] { proxyKernel };
        }

    }
}
