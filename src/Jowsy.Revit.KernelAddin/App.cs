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
            // throw new NotImplementedException();
        }

        public Result OnStartup(UIControlledApplication application)
        {
            AppDomain.CurrentDomain.AssemblyResolve
     += CurrentDomain_AssemblyResolve;

            //Assembly.LoadFrom("C:\\git\\bim-net-interactive\\src\\RevitKernelUI\\bin\\Debug R24\\Microsoft.DotNet.Interactive.Formatting.dll");
            var str = Microsoft.DotNet.Interactive.Formatting.JsonFormatter.MimeType;

            var ribbonPanel = application.CreateRibbonPanel("NET Interactive");
            //var str2 = new Microsoft.DotNet.Interactive.Formatting.PocketView();

            /*var entryCommand = typeof(EntryCommand);
            var entryButtonData = new PushButtonData(entryCommand.FullName, "Start Kernel\n (Modeless)", Assembly.GetAssembly(entryCommand).Location, entryCommand.FullName); 
            ribbonPanel.AddItem(entryButtonData);*/

            var showCommand = typeof(ShowCommand);
            var showButtonData = new PushButtonData(showCommand.FullName, "Show\nDockable Pane)", Assembly.GetAssembly(showCommand).Location, showCommand.FullName);
            ribbonPanel.AddItem(showButtonData);

            KernelEventHandler = new RevitKernelExternalEventHandler();
            KernelEvent = ExternalEvent.Create(KernelEventHandler);

            KernelInitEventHandler = new KernelInitEventHandler();
            InitKernelEvent = ExternalEvent.Create(KernelInitEventHandler);

            var kernelPaneProvider = new KernelDockablePaneProvider(new ViewModel());
            application.RegisterDockablePane(DockablePaneId, "NETInteractive Revit Kernel", kernelPaneProvider);

           Formatter.SetPreferredMimeTypesFor(typeof(Element), "text/html");

            Formatter.Register(
                type: typeof(Document),
                formatter: (list, writer) =>
                {
       
                        writer.WriteLine($"{list.ToString()}");
        
                }, "text/plain");
            //It's common for object graphs to contain reference cycles. The .NET Interactive formatter will traverse object graphs but in order to avoid both oversized outputs and possible infinite recursion when there is a reference cycle, the formatter will only recurse to a specific depth.
            Formatter.RecursionLimit = 3;
            return Result.Succeeded;
        }



        Assembly
    CurrentDomain_AssemblyResolve(
      object sender,
      ResolveEventArgs args)
        {
            try
            {
                if (args.Name.Contains("Encodings") || args.Name.Contains("Retro"))
                {
                    string filename = Path.GetDirectoryName(typeof(ViewModel).Assembly.Location);

                    filename = Path.Combine(filename,
                      args.Name.Split(',').FirstOrDefault() + ".dll");

                    if (File.Exists(filename))
                    {
                        return Assembly
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
