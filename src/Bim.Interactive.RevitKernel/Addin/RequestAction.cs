using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sweco.Revit.CQBimRevitConnector;
public class RequestAction : IRequestAction
{
    public int ActionCode { get; set; }
    public object ActionData { get; set; }

}
