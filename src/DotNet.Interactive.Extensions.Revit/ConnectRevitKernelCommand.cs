using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Connection;
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

            proxyKernel.UseValueSharing();

            //Add support for sendvalue
            proxyKernel.KernelInfo.SupportedKernelCommands.Add(new KernelCommandInfo("SendValue"));

            return new ProxyKernel[] { proxyKernel };
        }

    }
}
