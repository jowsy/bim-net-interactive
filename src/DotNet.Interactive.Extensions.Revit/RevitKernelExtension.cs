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
            KernelInvocationContext.Current?.Display(
               new HtmlString(@"<details><summary>Write, compile and run Revit C# code.</summary></details>"));
            
            kernel.VisitSubkernelsAndSelf(kernel =>
            {
                if (kernel is CSharpKernel cSharpKernel)
                {
                    cSharpKernel.UseRevit("2024");
                }
            });
            /*
            
                if (kernel is not CompositeKernel compositeKernel) return Task.CompletedTask;

                var revitKernel = new RevitKernel("RevitCSharpKernel");
                
            compositeKernel.Add(revitKernel);
           // revitKernel.SubmitCodeAsync("#!csharp");
            KernelInvocationContext.Current?.Display(
                new HtmlString(@"<details><summary>Write, compile and run Revit C# code.</summary></details>"));
          */
            return Task.CompletedTask;
            }
    }
}
