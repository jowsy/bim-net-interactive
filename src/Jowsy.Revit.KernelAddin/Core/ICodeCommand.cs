
/*
 * Give credit to https://github.com/ricaun/RevitAddin.CodeCompileTest/blob/master/RevitAddin.CodeCompileTest/Services/CodeDomService.cs
 * */
using Autodesk.Revit.UI;

namespace Jowsy.Revit.KernelAddin.Core
{
    public interface ICodeCommand
    {
        public event EventHandler<DisplayEventArgs> OnDisplay;
        public (string, object) Execute(UIApplication uiapp, Variables variables);
    }
}

