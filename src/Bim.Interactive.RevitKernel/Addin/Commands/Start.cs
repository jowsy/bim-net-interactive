using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Sweco.Revit.CQBimRevitConnector.Commands;
[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class Start : IExternalCommand
{
    public const string CommandName = "Start";

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        throw new NotImplementedException();
    }

}