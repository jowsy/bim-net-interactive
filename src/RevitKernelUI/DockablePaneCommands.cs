using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitKernel;
using RevitKernel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitKernelUI
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
