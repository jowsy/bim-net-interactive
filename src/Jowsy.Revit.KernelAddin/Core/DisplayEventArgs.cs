/*
 * Give credit to https://github.com/ricaun/RevitAddin.CodeCompileTest/blob/master/RevitAddin.CodeCompileTest/Services/CodeDomService.cs
 * */
namespace Jowsy.Revit.KernelAddin.Core
{
    public class DisplayEventArgs : EventArgs
    {
        private readonly object _displayObject;

        public DisplayEventArgs(object displayObject)
        {
            _displayObject = displayObject;
        }

        public object DisplayObject
        {
            get { return _displayObject; }
        }
    }

}

