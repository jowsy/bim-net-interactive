using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Interactive.Extensions.Revit
{
    public sealed class CSharpKernelExtension : IKernelExtension
    {
        public Task OnLoadAsync(Kernel kernel)
        {
            if (kernel is not CompositeKernel compositeKernel) return Task.CompletedTask;

            compositeKernel
                .AddKernelConnector(new ConnectRevitKernelCommand());

            KernelInvocationContext.Current?.Display(
    new HtmlString(@"<details><summary>Revit C# Kernel Added.</summary></details>"));
          
            return Task.CompletedTask;
            }
    }
}
