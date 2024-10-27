using Autodesk.Revit.UI;

namespace Jowsy.Revit.KernelAddin.Core
{
    internal class KernelInitEventHandler : IExternalEventHandler
    {
        internal UIApplication UIApplication { get; set; }
        internal TaskCompletionSource<bool> Tcs = null;
        public void Execute(UIApplication app)
        {
            UIApplication = app;
            Tcs.SetResult(true);
        }

        public string GetName()
        {
            return nameof(KernelInitEventHandler);
        }
    }
}
