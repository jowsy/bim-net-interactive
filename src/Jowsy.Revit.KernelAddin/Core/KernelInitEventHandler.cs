using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
