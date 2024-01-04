using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RevitKernel.Core
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
