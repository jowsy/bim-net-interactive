using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using IRevitKernel.Core;
using Microsoft.DotNet.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitKernel.Core
{
    internal class RevitKernelExternalEventHandler : IExternalEventHandler
    {
        internal bool IsRunning { get; set; }

        internal TaskCompletionSource<bool> tcs = null;

        public KernelInvocationContext KernelContext { get; set; }
        public string SubmitCode { get; set; }
        public void Execute(UIApplication uiapp)
            {
            try
            {
                IsRunning = true;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                if (null == uidoc)
                {
                    return; // no document, nothing to do
                }
                try
    {
                    var service = new CodeDomService();
                    var newCommand = service.CreateCommand(KernelContext, (string)SubmitCode);

                    if (newCommand == null)
                    {
                        //tcs.SetResult(true);
                        return;
                    }
                    var type = newCommand.GetType("CodeNamespace.Command");
                    var runnable = Activator.CreateInstance(type) as ICodeCommand;
                    if (runnable == null) throw new Exception("");
                    runnable.OnDisplay += Runnable_OnDisplay1;
                    runnable.Execute(uiapp);
                    //context.DisplayStandardOut("Code were successfully compiled and executed in the Revit thread.");
                }
                catch (Exception ex)
                {

                    KernelContext.DisplayStandardError("Compilation failed: \n" + ex.ToString());
                    }
            }
            finally
            {
                tcs.SetResult(true);
            }
                IsRunning = false;
            }

        private void Runnable_OnDisplay1(object sender, DisplayEventArgs e)
        {
            KernelContext.DisplayStandardOut(e.DisplayObject.ToString());
        }

        public string GetName()
            {
                return "my event";
            }
        
    }
}
