using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitKernel.Core
{
    internal class RevitKernelExternalEventHandler : IExternalEventHandler
    {
            public void Execute(UIApplication uiapp)
            {
                UIDocument uidoc = uiapp.ActiveUIDocument;
                if (null == uidoc)
                {
                    return; // no document, nothing to do
                }
                Document doc = uidoc.Document;
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("MyEvent");
                    // Action within valid Revit API context thread
                    tx.Commit();
                }
            }
            public string GetName()
            {
                return "my event";
            }
        
    }
}
