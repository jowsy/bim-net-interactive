
/*
 * Give credit to https://github.com/ricaun/RevitAddin.CodeCompileTest/blob/master/RevitAddin.CodeCompileTest/Services/CodeDomService.cs
 * */
using Autodesk.Revit.UI;

namespace IRevitKernel.Core
{
    public class DisplayEventArgs : EventArgs
    {
        private readonly object _displayObject;

        public DisplayEventArgs(Object displayObject)
        {
            _displayObject = displayObject;
        }

        public object DisplayObject
        {
            get { return _displayObject; }
        }
    }
    public interface ICodeCommand
        {
        public event EventHandler<DisplayEventArgs> OnDisplay;
        public bool Execute(UIApplication uiapp);
        }
}

