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
            /*
         KernelInvocationContext.Current?.Display(
            new HtmlString(@"<details><summary>Write, compile and run Revit C# code.</summary></details>"));

         kernel.VisitSubkernelsAndSelf(kernel =>
         {
             if (kernel is CSharpKernel cSharpKernel)
             {
                 cSharpKernel.UseRevit("2024");

                 //await cSharpKernel.SubmitCodeAsync("using DotNet.Interactive.Extensions.Revit;");
                 //await cSharpKernel.SubmitCodeAsync("using Autodesk.Revit.DB;");
                 //await cSharpKernel.SubmitCodeAsync("using Autodesk.Revit.UI;");
                // await cSharpKernel.SubmitCodeAsync("UIDocument uidoc = default;");
             }
         });
            */

           // kernel.SubmitCodeAsync("#r \"nuget:Revit.RevitApi.x64, 2023.0.0\"");
            //await cSharpKernel.SubmitCodeAsync("#r \"nuget:Revit.RevitApi.x64, 2023.0.0\"");


            /* if (kernel is not CompositeKernel compositeKernel) return Task.CompletedTask;

             var revitKernel = new RevitKernel("RevitCSharpKernel");

             compositeKernel.Add(revitKernel);
             revitKernel.UseValueSharing();

             KernelInvocationContext.Current?.Display(
                 new HtmlString(@"<details><summary>Revit C# Kernel Added.</summary></details>"));
      */
            return Task.CompletedTask;
            }
    }
}
