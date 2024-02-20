using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Jowsy.Revit.KernelAddin.Core;
using Jowsy.Revit.KernelAddin.UI;
using Microsoft.DotNet.Interactive.Formatting;
using System.IO;
using System.Reflection;

namespace Jowsy.Revit.KernelAddin
{
    public class App : IExternalApplication
    {
        public static ExternalEvent KernelEvent;
        public static ExternalEvent InitKernelEvent;

        public static DockablePaneId DockablePaneId = new DockablePaneId(new Guid("3776E883-839D-4D43-8498-8C7D2345C1CB"));
        internal static RevitKernelExternalEventHandler KernelEventHandler;
        internal static KernelInitEventHandler KernelInitEventHandler;

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Cancelled;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var ribbonPanel = application.CreateRibbonPanel("NET Interactive");

            var showCommand = typeof(ShowCommand);
            var showButtonData = new PushButtonData(showCommand.FullName, "Show\nDockable Pane)", Assembly.GetAssembly(showCommand).Location, showCommand.FullName);
            ribbonPanel.AddItem(showButtonData);

            KernelEventHandler = new RevitKernelExternalEventHandler();
            KernelEvent = ExternalEvent.Create(KernelEventHandler);

            KernelInitEventHandler = new KernelInitEventHandler();
            InitKernelEvent = ExternalEvent.Create(KernelInitEventHandler);

            var kernelPaneProvider = new KernelDockablePaneProvider(new ViewModel(application.ControlledApplication.VersionNumber));

            application.RegisterDockablePane(DockablePaneId, "NETInteractive Revit Kernel", kernelPaneProvider);

            //TODO: Implement a better formatter! Maybe based on RevitLookup?
           Formatter.SetPreferredMimeTypesFor(typeof(Element), "text/html");

            //It's common for object graphs to contain reference cycles.
            //The .NET Interactive formatter will traverse object graphs but in order to avoid both oversized outputs and possible
            //infinite recursion when there is a reference cycle, the formatter will only recurse to a specific depth.
            Formatter.RecursionLimit = 3;
            return Result.Succeeded;
        }
        Assembly CurrentDomain_AssemblyResolve(object sender,ResolveEventArgs args)
        {
            // load the assembly from the embedded resources
            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyPath = Path.Combine(folder, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            return null;
        }
    }
}
