using Autodesk.Revit.UI;
using RevitKernel.Core;
using RevitKernel.UI;
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

        public static DockablePaneId DockablePaneId = new DockablePaneId(new Guid("3776E883-839D-4D43-8498-8C7D2345C1CB"));
        internal static RevitKernelExternalEventHandler KernelEventHandler;
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Cancelled;
            // throw new NotImplementedException();
        }

        public Result OnStartup(UIControlledApplication application)
        {
            var ribbonPanel = application.CreateRibbonPanel("NET Interactive");

            /*var entryCommand = typeof(EntryCommand);
            var entryButtonData = new PushButtonData(entryCommand.FullName, "Start Kernel\n (Modeless)", Assembly.GetAssembly(entryCommand).Location, entryCommand.FullName); 
            ribbonPanel.AddItem(entryButtonData);*/

            var showCommand = typeof(ShowCommand);
            var showButtonData = new PushButtonData(showCommand.FullName, "Show\nDockable Pane)", Assembly.GetAssembly(showCommand).Location, showCommand.FullName);
            ribbonPanel.AddItem(showButtonData);

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveDlls);

            KernelEventHandler = new RevitKernelExternalEventHandler();
            ExternalEvent = ExternalEvent.Create(KernelEventHandler);


            var kernelPaneProvider = new KernelDockablePaneProvider(new ViewModel());
            application.RegisterDockablePane(App.DockablePaneId, "NETInteractive Revit Kernel", kernelPaneProvider);


            return Result.Succeeded;
        }

        private static Assembly ResolveDlls(object sender, ResolveEventArgs args)
        {
            try
            {
                if (args.Name.ToLower().Contains("system.diagnostics.diagnosticsource") || args.Name.Contains("System.Reflection.Metadata") || args.Name.Contains("Microsoft.CodeAnalysis"))
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
