using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Jowsy.Revit.KernelAddin
{

    [Transaction(TransactionMode.Manual)]
    public class ShowCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DockablePane dockableWindow = commandData.Application.GetDockablePane(App.DockablePaneId);


            /*   if (!KernelDockablePaneProvider._viewModel.KernelIsRunning())
               {
                   KernelDockablePaneProvider._viewModel.InitKernel();
               }*/

            dockableWindow.Show();



            return Result.Succeeded;
        }
    }



}
