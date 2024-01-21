using Elfie.Serialization;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jowsy.CSharp.Tests
{
    public class TestBase
    {

        private ReferenceList _references;
        private ReferenceList GetMetadataReferences()
        {        
                if (_references == null)
                {
                InitReferences();
                }
            return _references;
        }

        internal Compilation? Compile(string sourceText)
        {

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceText.Trim());
            var optimizationLevel = OptimizationLevel.Release;

            var compilation = CSharpCompilation.Create($"default-test-library")
                .WithOptions(new CSharpCompilationOptions(
                            OutputKind.DynamicallyLinkedLibrary, optimizationLevel: optimizationLevel)
                )
                .AddReferences(GetMetadataReferences())
                .AddSyntaxTrees(syntaxTree);

            return compilation;

        }

        public void InitReferences()
        {
            _references = new ReferenceList();
            AddAssembly("mscorlib.dll");
            AddAssembly("System.dll");
            AddAssembly("System.Core.dll");
            //AddAssembly("System.Private.CoreLib.dll");
            AddAssembly("Microsoft.CSharp.dll");
            AddAssembly("System.Net.Http.dll");
            //AddAssembly(typeof(object).GetTypeInfo().Assembly.FullName);
            AddAssembly("C:\\git\\bim-net-interactive\\src\\RevitKernelUI\\bin\\Debug R24\\Jowsy.Revit.KernelAddin.dll");
            AddAssembly("C:\\Program Files\\Autodesk\\Revit 2024\\RevitAPI.dll");
            AddAssembly("C:\\Program Files\\Autodesk\\Revit 2024\\RevitAPIUI.dll");

            // this library and CodeAnalysis libs
            //AddAssembly(typeof(ReferenceList)); // Scripting Library
        }
        internal bool AddAssembly(string assemblyDll)
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

            if (_references.Any(r => r.FilePath == file)) return true;

            try
            {
                var reference = MetadataReference.CreateFromFile(file);
                _references.Add(reference);
            }
            catch
            {
                return false;
            }

            return true;
        }
        public class ReferenceList : HashSet<PortableExecutableReference>
        {

        }
    }
}
