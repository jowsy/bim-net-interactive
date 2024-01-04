
/*
 * Give credit to https://github.com/ricaun/RevitAddin.CodeCompileTest/blob/master/RevitAddin.CodeCompileTest/Services/CodeDomService.cs
 * */
using Autodesk.Revit.UI;

namespace IRevitKernel.Core
{
    public interface ICodeCommand
        {
            public bool Execute(UIApplication uiapp);
        }
}

