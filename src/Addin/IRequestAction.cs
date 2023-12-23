namespace Sweco.Revit.CQBimRevitConnector;

public interface IRequestAction
{
    public int ActionCode { get; set; }
    public object ActionData { get; set; }
}