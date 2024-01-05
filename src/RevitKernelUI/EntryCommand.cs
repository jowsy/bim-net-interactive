using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitKernelUI
{
    [Transaction(TransactionMode.Manual)]
    public class EntryCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var window = new MainWindow(commandData.Application.MainWindowHandle, uiapp);
            window.Show();    
            return Result.Succeeded;
        }
    }
}
