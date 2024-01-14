using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Connection;
using Microsoft.DotNet.Interactive.Events;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jowsy.DotNet.Interactive.Extensions
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


            proxyKernel.AddMiddleware(async (command, context, next) =>
            {
                var submitCommand = command as SubmitCode;
                if (submitCommand != null)
                {
                    RoslynCompilerService roslynCompilerService = new RoslynCompilerService();

                    context.DisplayStandardOut($"Requesting value infos...");
                    var result = await proxyKernel.SendAsync(new RequestValueInfos());

                    var valueInfosProduced = result.Events.Where(e => e is ValueInfosProduced)
                                                   .FirstOrDefault() as ValueInfosProduced;

                    if (valueInfosProduced != null) {
                        context.DisplayStandardOut($"Value info received...");
                        foreach (var item in valueInfosProduced.ValueInfos)
                        {
                            context.DisplayStandardOut($"Name: {item.Name}, TypeName: {item.TypeName}: Value:{item.FormattedValue.Value}\n");
                        }
                    }

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
            proxyKernel.KernelInfo.SupportedKernelCommands.Add(new KernelCommandInfo("RequestValueInfos"));

            var reqHover = proxyKernel.KernelInfo.SupportedKernelCommands.Where(c => c.Name == "RequestHoverText").FirstOrDefault();
            if (reqHover != null)
            {
                proxyKernel.KernelInfo.SupportedKernelCommands.Remove(reqHover);
            }

            return new ProxyKernel[] { proxyKernel };
        }

    }
}
