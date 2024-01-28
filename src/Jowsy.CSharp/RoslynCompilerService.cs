using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.Interactive.ValueSharing;
using Microsoft.DotNet.Interactive.Connection;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;

//Influenced by https://github.com/RickStrahl/Westwind.Scripting/blob/master/Westwind.Scripting/CSharpScriptExecution.cs#L1254
namespace Jowsy.CSharp
{
    public class RoslynCompilerService
    {
        public ReferenceList References { get; private set; }
        public string GeneratedClassCode { get; private set; }

        public RoslynCompilerService()
        {
            References = new ReferenceList();
            AddNetFrameworkDefaultReferences();
        }
        public void AddNetFrameworkDefaultReferences()
        {
            AddAssembly("mscorlib.dll");
            AddAssembly("System.dll");
            AddAssembly("System.Core.dll");
            //AddAssembly("System.Private.CoreLib.dll");
            AddAssembly("Microsoft.CSharp.dll");
            AddAssembly("System.Net.Http.dll");
            //AddAssembly(typeof(object).GetTypeInfo().Assembly.FullName);
            AddAssembly("C:\\git\\bim-net-interactive\\src\\Jowsy.Revit.KernelAddin\\bin\\Debug R24\\Jowsy.Revit.KernelAddin.dll");
            AddAssembly("C:\\Program Files\\Autodesk\\Revit 2024\\RevitAPI.dll");
            AddAssembly("C:\\Program Files\\Autodesk\\Revit 2024\\RevitAPIUI.dll");
            AddAssembly("C:\\Program Files\\Autodesk\\Revit 2024\\RevitAPIIFC.dll");
           
            AddAssembly(typeof(ReferenceList)); // Scripting Library
        }
        public bool AddAssembly(string assemblyDll)
        {
            if (string.IsNullOrEmpty(assemblyDll)) return false;

            var file = Path.GetFullPath(assemblyDll);

            if (!File.Exists(file))
            {
                // check framework or dedicated runtime app folder
                var path = "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.8\\";//Path.GetDirectoryName(typeof(object).Assembly.Location);
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


        /// <summary>
        /// Compiles a revit addin dll from a C#-script
        /// </summary>
        /// <param name="script">C# code</param>
        /// <param name="KernelValueInfosResolver">a resolver for list of kernelvalueinfos</param>
        /// <returns></returns>
        public async Task<CompilationResults> CompileRevitAddin(string script, bool 
                                                                toAssemblyFile, 
                                                                Func<Task<KernelValueInfo[]>>?  KernelValueInfosResolver)
        {
            var source = SyntaxUtils.BuildClassCode(script);

            var tree = CSharpSyntaxTree.ParseText(source.Trim());

            var root = (CompilationUnitSyntax)tree.GetRoot();
            
            var tRoot = SyntaxUtils.FixReturn(root);
            var finalTree = CSharpSyntaxTree.Create(tRoot);
           
            var optimizationLevel = OptimizationLevel.Release;

            var compilation = CSharpCompilation.Create($"revitkernelgenerated-{DateTime.Today.Ticks}")
                .WithOptions(new CSharpCompilationOptions(
                            OutputKind.DynamicallyLinkedLibrary, optimizationLevel: optimizationLevel)
                )
                .AddReferences(References)
                .AddSyntaxTrees(finalTree);

            var diagnostics = compilation.GetDiagnostics().Where(d => d.Id == "CS0103"); //Look for undeclared variables

            GeneratedClassCode = finalTree.ToString();

            if (diagnostics.Any()) {

                if (KernelValueInfosResolver == null)
                {
                    throw new ArgumentNullException(nameof(KernelValueInfosResolver));  
                }

                var valueInfos = await KernelValueInfosResolver();
             
                    var syntax = SyntaxUtils.ResolveUndeclaredVariables(compilation, valueInfos) as CompilationUnitSyntax;

                    //New try
                    var newTree = CSharpSyntaxTree.Create(syntax);
                    compilation = CSharpCompilation.Create($"revitkernelgenerated-{DateTime.Today.Ticks}")
                        .WithOptions(new CSharpCompilationOptions(
                                    OutputKind.DynamicallyLinkedLibrary, optimizationLevel: optimizationLevel)
                        )
                        .AddReferences(References)
                        .AddSyntaxTrees(newTree);

                GeneratedClassCode = newTree.ToString();    

            }
            //if (SaveGeneratedCode)
           


            Stream codeStream = null;
            Assembly assembly = null;
            string outputAssembly = null;

            if (toAssemblyFile)
            {
                outputAssembly = Path.Combine(Path.GetTempPath(),
                                  "revitkernel", $"{Path.GetRandomFileName()}.dll");
                if (!Directory.Exists(Path.GetDirectoryName(outputAssembly)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputAssembly));
                }
            }

            codeStream = toAssemblyFile ? new FileStream(outputAssembly, FileMode.Create) :
                                          new MemoryStream();
           

            
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
                    if (sb != null && sb.Length > 0)
                    {
                        return new CompilationResults()
                        {
                            DiagnosticText = sb.ToString(),
                            Success = false
                        };
                        //   context.DisplayStandardError(sb.ToString());
                    }
                    //ErrorType = ExecutionErrorTypes.Compilation;
                    //ErrorMessage = sb.ToString();
                    if (!toAssemblyFile)
                    {
                        var memStream = (MemoryStream)codeStream;
                        memStream.Seek(0, SeekOrigin.Begin);
                        assembly = Assembly.Load(memStream.ToArray());
                    }
                    // no exception here during compilation - return the error
                    //SetErrors(new ApplicationException(ErrorMessage), true);
                    return new CompilationResults()
                    {
                        Success = false,
                        DiagnosticText = sb.ToString(),
                    };
                }
            }

            if (toAssemblyFile)
            {
                return new CompilationResults()
                {
                    Success = true,
                    AssemblyPath = outputAssembly
                };
            }
            else
            {
                return new CompilationResults()
                {
                    Success = true,
                    Assembly = assembly
            };
            }


        }
        private Assembly LoadAssemblyFrom(string assemblyFile)
        {
#if NETCORE
            if (AlternateAssemblyLoadContext != null)
            {
                return AlternateAssemblyLoadContext.LoadFromAssemblyPath(assemblyFile);
            }
#endif
            return Assembly.LoadFrom(assemblyFile);
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
