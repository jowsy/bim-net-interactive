using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jowsy.Revit.KernelAddin.Core
{
    internal class ScriptSample : ICodeCommand
    {
        public event EventHandler<DisplayEventArgs> OnDisplay;

        public (string, object) Execute(UIApplication uiapp)
        {
            //Because the revit kernel is run out of process embedded in the revit application we need to use the set magic command
            // UIDocument uidoc = default;
            var uidoc = uiapp.ActiveUIDocument;

            Document doc = uidoc.Document;

            // Set export options
            IFCExportOptions ifcExportOptions = new IFCExportOptions();


            // Optionally, you can customize export options further, e.g., set specific configurations.
            // ifcExportOptions.SpaceBoundaries = IFCExportSpaceBoundaries.Boundaries;
            // ifcExportOptions.SplitWallsAndColumns = true;
            // ...

            // Perform the export
            Transaction transaction = new Transaction(doc, "Export IFC");
            transaction.Start("start");



            // Use the Export method of the Document class
            doc.Export("c:\\Temp\\", "export2.ifc", ifcExportOptions);

            transaction.Commit();


            return (null, null);
            //  transaction

        }

        public (string, object) Execute(UIApplication uiapp, Variables variables)
        {
            throw new NotImplementedException();
        }
    }
}
