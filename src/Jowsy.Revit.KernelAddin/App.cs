using Autodesk.Revit.UI;
using Jowsy.Revit.KernelAddin.Core;
using Jowsy.Revit.KernelAddin.UI;
using System.IO;
using System.Reflection;

namespace Jowsy.Revit.KernelAddin
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
            ExternalEvent = ExternalEvent.Create(KernelEventHandler);


            var kernelPaneProvider = new KernelDockablePaneProvider(new ViewModel());
            application.RegisterDockablePane(DockablePaneId, "NETInteractive Revit Kernel", kernelPaneProvider);


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
