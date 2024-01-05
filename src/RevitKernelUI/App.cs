using Autodesk.Revit.UI;
using RevitKernel.Core;
using RevitKernelUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitKernel
{
    public class App : IExternalApplication
    {
        public static ExternalEvent ExternalEvent;


        internal static RevitKernelExternalEventHandler KernelEventHandler;
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Cancelled;
            // throw new NotImplementedException();
        }

        public Result OnStartup(UIControlledApplication application)
        {
            var ribbonPanel = application.CreateRibbonPanel("NET Interactive");

            var command = typeof(EntryCommand);
            var buttonData = new PushButtonData(command.FullName, "Start Kernel", Assembly.GetAssembly(command).Location, command.FullName);
         
            ribbonPanel.AddItem(buttonData);

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveDlls);

            KernelEventHandler = new RevitKernelExternalEventHandler();
            ExternalEvent = ExternalEvent.Create(KernelEventHandler);

            return Result.Succeeded;
        }

        private static Assembly ResolveDlls(object sender, ResolveEventArgs args)
        {
            try
            {
                if (args.Name.ToLower().Contains("system.diagnostics.diagnosticsource"))
                {
                    string filename = Path.GetDirectoryName(typeof(ViewModel).Assembly.Location);

                    filename = Path.Combine(filename,
                      args.Name.Split(',').FirstOrDefault() + ".dll");

                    if (File.Exists(filename))
                    {
                        return System.Reflection.Assembly
                          .LoadFrom(filename);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}
