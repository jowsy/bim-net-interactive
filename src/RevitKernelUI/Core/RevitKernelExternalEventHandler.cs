﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using IRevitKernel.Core;
using Microsoft.DotNet.Interactive;
using RevitKernelUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitKernel.Core
{
    internal class RevitKernelExternalEventHandler : IExternalEventHandler
    {
        internal bool IsRunning { get; set; }

        internal TaskCompletionSource<(string,object)> tcs = null;

        public KernelInvocationContext KernelContext { get; set; }
        public string CompiledDllPath { get; set; }
        public void Execute(UIApplication uiapp)
            {
            (string, object) result = (null, null);
            try
            {
                //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveDlls);
                IsRunning = true;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                if (null == uidoc)
                {
                    return; // no document, nothing to do
                }
                try
    {
                    var newCommand = Assembly.LoadFile(CompiledDllPath);
                    if (newCommand == null)
                    {
                        return;
                    }
                    var type = newCommand.GetType("CodeNamespace.Command");
                    var runnable = Activator.CreateInstance(type) as ICodeCommand;
                    if (runnable == null) throw new Exception("");
                    runnable.OnDisplay += Runnable_OnDisplay1;
                    result = runnable.Execute(uiapp);
                    //context.DisplayStandardOut("Code were successfully compiled and executed in the Revit thread.");
                }
                catch (Exception ex)
                {
                    KernelContext.DisplayStandardError("Compilation failed: \n" + ex.ToString());
                }
            }
            finally
            {
                tcs.SetResult(result);
            }
                IsRunning = false;
            }

        private Assembly ResolveDlls(object sender, ResolveEventArgs args)
        {

            try
            {
                if (args.Name.Contains("System.Reflection.Metadata") || args.Name.Contains("Microsoft.CodeAnalysis"))
                {
                    string filename = Path.GetDirectoryName(typeof(ViewModel).Assembly.Location);

                    filename = Path.Combine(filename,
                      args.Name.Split(',').FirstOrDefault() + ".dll");

                    if (File.Exists(filename))
                    {
                        return System.Reflection.Assembly
                          .LoadFrom(filename);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return null;
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
