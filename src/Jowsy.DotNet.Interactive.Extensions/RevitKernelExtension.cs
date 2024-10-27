using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;

namespace Jowsy.DotNet.Interactive.Extensions
{
    public sealed class CSharpKernelExtension : IKernelExtension
    {
        public Task OnLoadAsync(Kernel kernel)
        {
            if (kernel is not CompositeKernel compositeKernel) return Task.CompletedTask;

            compositeKernel
                .AddKernelConnector(new ConnectRevitKernelCommand());

            KernelInvocationContext.Current?.Display(
          new HtmlString(@"<details><summary>Autodesk Revit Interactive Extension</summary>
<p>This extension adds support for connecting to Autodesk Revit using the <code>#!connect revit</code> magic command. As example, to connect to revit 2024, run a cell using <code>#!connect revit --kernel-name revit24 --revit-version 2024</code>.</p>
<p>Then begin with #!revit24 in a C#-cell to execute code in the revit context.</p></details>"), "text/html");

            return Task.CompletedTask;
        }
    }
}
