
/*
 * Give credit to https://github.com/ricaun/RevitAddin.CodeCompileTest/blob/master/RevitAddin.CodeCompileTest/Services/CodeDomService.cs
 * */
using Microsoft.CSharp;
using Microsoft.DotNet.Interactive;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Jowsy.Revit.KernelAddin.Core
{
    public class CodeDomService
    {
        public Assembly CreateCommand(KernelInvocationContext context, string commandCode)
        {
            string source = @"
            using System;
            using Autodesk.Revit.Attributes;
            using Autodesk.Revit.DB;
            using Autodesk.Revit.UI;
            using System.Collections;
            using System.Linq;
            using System.Collections.Generic;
            using IRevitKernel.Core;

            namespace CodeNamespace 
            {
              public class Command : ICodeCommand
              {     

                public event EventHandler<DisplayEventArgs> OnDisplay;
                
                public bool Execute(UIApplication uiapp)
                {
                    UIDocument uidoc = uiapp.ActiveUIDocument;"
+ commandCode + @"
                 return true;
                }

                public void display(Object o)
                {
                    OnDisplay?.Invoke(this, new DisplayEventArgs(o));
                }
              }
            }";
            var targetUnit = new CodeSnippetCompileUnit(source);

            return GenerateCode(context, targetUnit);

        }


        public Assembly GenerateCode(KernelInvocationContext context, params CodeCompileUnit[] compilationUnits)
        {
            CodeDomProvider provider = new CSharpCodeProvider();
            CompilerParameters compilerParametes = new CompilerParameters();

            compilerParametes.GenerateExecutable = false;
            compilerParametes.IncludeDebugInformation = false;
            compilerParametes.GenerateInMemory = false;

            #region Add GetReferencedAssemblies
            var assemblyNames = Assembly.GetExecutingAssembly().GetReferencedAssemblies().ToList();
            assemblyNames.Add(Assembly.GetExecutingAssembly().GetName());

            var nameAssemblies = new Dictionary<string, Assembly>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assemblyNames.Any(e => e.Name == assembly.GetName().Name))
                {
                    nameAssemblies[assembly.GetName().Name] = assembly;
                }
            }
            foreach (var keyAssembly in nameAssemblies)
            {
                //Console.WriteLine($"Assembly: {keyAssembly.Key}");
                compilerParametes.ReferencedAssemblies.Add(keyAssembly.Value.Location);
            }
            #endregion

            CompilerResults results = provider.CompileAssemblyFromDom(compilerParametes, compilationUnits);

            if (results.Errors != null && results.Errors.Count > 0)
            {
                foreach (var item in results.Errors)
                {
                    context.DisplayStandardError(item.ToString() + "\n");
                }
            }

            if (results.Errors.Count > 0)
            {
                return null;
            }
            return results.CompiledAssembly;
        }
    }
}

