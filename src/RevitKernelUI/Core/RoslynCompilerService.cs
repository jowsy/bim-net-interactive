using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Interactive;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Emit;

namespace RevitKernel.Core
{
    public class ReferenceList : HashSet<PortableExecutableReference>
    {

    }
    public class RoslynCompilerService
    {
        public RoslynCompilerService() {

            var assemblyNames = Assembly.GetExecutingAssembly().GetReferencedAssemblies().ToList();
            foreach (var assembly in assemblyNames)
            {
                AddAssembly(assembly.Name);
            }
            //AddNetFrameworkDefaultReferences();
        }
        public void AddNetFrameworkDefaultReferences()
        {
            AddAssembly("mscorlib.dll");
            AddAssembly("System.dll");
            AddAssembly("System.Core.dll");
            AddAssembly("Microsoft.CSharp.dll");
            AddAssembly("System.Net.Http.dll");

            // this library and CodeAnalysis libs
            AddAssembly(typeof(ReferenceList)); // Scripting Library
        }
        public bool AddAssembly(string assemblyDll)
        {
            if (string.IsNullOrEmpty(assemblyDll)) return false;

            var file = Path.GetFullPath(assemblyDll);

            if (!File.Exists(file))
            {
                // check framework or dedicated runtime app folder
                var path = Path.GetDirectoryName(typeof(object).Assembly.Location);
                file = Path.Combine(path, assemblyDll);
                if (!File.Exists(file))
                    return false;
            }

            if (References.Any(r => r.FilePath == file)) return true;

            try
            {
                var reference = MetadataReference.CreateFromFile(file);
                References.Add(reference);
            }
            catch
            {
                return false;
            }

            return true;
        }


        public ReferenceList References { get; private set; }
        public string GeneratedClassCode { get; private set; }

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


            var tree = SyntaxFactory.ParseSyntaxTree(source.Trim());

            var optimizationLevel =  OptimizationLevel.Release;


            var compilation = CSharpCompilation.Create("revitkernelgenerated")
                .WithOptions(new CSharpCompilationOptions(
                            OutputKind.DynamicallyLinkedLibrary)
                )
                .AddReferences(References)
                .AddSyntaxTrees(tree);

            //if (SaveGeneratedCode)
                GeneratedClassCode = tree.ToString();

            bool isFileAssembly = false;
            Stream codeStream = null;
           /* if (string.IsNullOrEmpty(OutputAssembly))
            {*/
                codeStream = new MemoryStream(); // in-memory assembly
            //}
            //else
            //{
             //   codeStream = new FileStream(OutputAssembly, FileMode.Create, FileAccess.Write);
             //   isFileAssembly = true;
            //}

            using (codeStream)
            {
                EmitResult compilationResult = null;
                /*if (CompileWithDebug)
                {
                    var debugOptions = CompileWithDebug ? DebugInformationFormat.Embedded : DebugInformationFormat.Pdb;
                    compilationResult = compilation.Emit(codeStream,
                        options: new EmitOptions(debugInformationFormat: debugOptions));
                }
                else*/
                compilationResult = compilation.Emit(codeStream);

                // Compilation Error handling
                if (!compilationResult.Success)
                {
                    var sb = new StringBuilder();
                    foreach (var diag in compilationResult.Diagnostics)
                    {
                        sb.AppendLine(diag.ToString());
                    }

                    //ErrorType = ExecutionErrorTypes.Compilation;
                    //ErrorMessage = sb.ToString();

                    // no exception here during compilation - return the error
                    //SetErrors(new ApplicationException(ErrorMessage), true);
                    return null;
                }
            }

            /*if (!noLoad)
            {
                if (!isFileAssembly)
                   
                else
                    Assembly = LoadAssemblyFrom(OutputAssembly);
            }*/
            return LoadAssembly(((MemoryStream)codeStream).ToArray());


        }

        private Assembly LoadAssembly(byte[] rawAssembly)
        {
/*#if NETCORE
            if (AlternateAssemblyLoadContext != null)
            {
                return AlternateAssemblyLoadContext.LoadFromStream(new MemoryStream(rawAssembly));
            }
#endif*/
            return Assembly.Load(rawAssembly);
        }


        public bool AddAssembly(Type type)
        {
            try
            {
                if (References.Any(r => r.FilePath == type.Assembly.Location))
                    return true;

                var systemReference = MetadataReference.CreateFromFile(type.Assembly.Location);
                References.Add(systemReference);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
