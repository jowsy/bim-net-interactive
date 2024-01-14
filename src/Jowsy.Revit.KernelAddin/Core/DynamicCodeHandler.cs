using Autodesk.Revit.UI;
using Jowsy.Revit.KernelAddin.Core;

namespace Jowsy.Revit.KernelAddin.Core
{
    internal class DynamicCodeHandler : IDisposable
    {
        private bool disposed = false;
        internal static ExternalEvent Event;

        internal DynamicCodeHandler()
        {
            var eventHandler = new RevitKernelExternalEventHandler();
            Event = ExternalEvent.Create(eventHandler);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                Event?.Dispose();
                Event = null;
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(false);
        }
    }
}
