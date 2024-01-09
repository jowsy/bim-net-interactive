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
using Microsoft.CodeAnalysis.CSharp.Syntax;
//Influenced by https://github.com/RickStrahl/Westwind.Scripting/blob/master/Westwind.Scripting/CSharpScriptExecution.cs#L1254
namespace RevitKernel.Core
{
    public class ReferenceList : HashSet<PortableExecutableReference>
    {

    }
    public class RoslynCompilerService
    {
        public RoslynCompilerService() {
            References = new ReferenceList();
            AddNetFrameworkDefaultReferences();
            /*var assemblyNames = Assembly.GetExecutingAssembly().GetReferencedAssemblies().ToList();
            foreach (var assembly in assemblyNames)
            {
                AddAssembly(assembly.Name);
            }*/
            //AddNetFrameworkDefaultReferences();
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
            AddAssembly("C:\\git\\bim-net-interactive\\src\\RevitKernelUI\\bin\\Debug R24\\RevitKernel.dll");
            AddAssembly("C:\\Program Files\\Autodesk\\Revit 2024\\RevitAPI.dll");
            AddAssembly("C:\\Program Files\\Autodesk\\Revit 2024\\RevitAPIUI.dll");

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


        public ReferenceList References { get; private set; }
        public string GeneratedClassCode { get; private set; }

        public string CompileCode(KernelInvocationContext context, string commandCode)
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
                
                public (string, object) Execute(UIApplication uiapp)
                {
                    UIDocument uidoc = uiapp.ActiveUIDocument;"
+ commandCode + @"
                }

                public void display(object o)
                {
                    OnDisplay?.Invoke(this, new DisplayEventArgs(o));
                }
              }
            }"
            ;

            var tree = CSharpSyntaxTree.ParseText(source.Trim());
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var tRoot = TransformRoot(root);
            var finalTree = CSharpSyntaxTree.Create(tRoot);
            //var tree = SyntaxFactory.ParseSyntaxTree(source.Trim());

            var optimizationLevel =  OptimizationLevel.Release;
    
            var compilation = CSharpCompilation.Create($"revitkernelgenerated-{DateTime.Today.Ticks}")
                .WithOptions(new CSharpCompilationOptions(
                            OutputKind.DynamicallyLinkedLibrary, optimizationLevel:optimizationLevel)
                )
                .AddReferences(References)
                .AddSyntaxTrees(finalTree);

            //if (SaveGeneratedCode)
                GeneratedClassCode = finalTree.ToString();

            bool isFileAssembly = false;
            Stream codeStream = null;
            /* if (string.IsNullOrEmpty(OutputAssembly))
             {*/
            //  codeStream = new MemoryStream(); // in-memory assembly
            //}
            //else
            //{
            string outputAssembly = Path.Combine(Path.GetTempPath(),
                                            "revitkernel", $"{Path.GetRandomFileName()}.dll");
            if (!Directory.Exists(Path.GetDirectoryName(outputAssembly))){
                Directory.CreateDirectory(Path.GetDirectoryName(outputAssembly));
            }

               codeStream = new FileStream(outputAssembly, FileMode.Create);
                isFileAssembly = true;
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
                    if (sb!= null && sb.Length > 0) {
                        context.DisplayStandardError(sb.ToString());
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
            return outputAssembly;


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

        private static CompilationUnitSyntax TransformRoot(CompilationUnitSyntax root)
        {

            MethodDeclarationSyntax methodDecl = root
    .DescendantNodes()
    .OfType<ClassDeclarationSyntax>()
    .First().ChildNodes().OfType<MethodDeclarationSyntax>().First();

            var block = methodDecl.DescendantNodes().OfType<BlockSyntax>().FirstOrDefault();

            //look for return statement in the method block
            var returnStatement = block.DescendantNodes()
                                       .OfType<ReturnStatementSyntax>().FirstOrDefault();

            if (returnStatement != null)
            {
                var returnStatementSyntax = returnStatement as ReturnStatementSyntax;
                var varName = (returnStatementSyntax.Expression as IdentifierNameSyntax).Identifier.Text;

                var newNode = SyntaxFactory.ParseStatement($"return (\"{varName}\", {varName});");
                var newTree2 = root.ReplaceNode(returnStatement, newNode);
                return newTree2;
            }

            var nodes = block.ChildNodes();
            var lastNode = nodes.Last();


            var testCSharpScriptReturnStatement = lastNode as ExpressionStatementSyntax;
            if (testCSharpScriptReturnStatement != null)
            {
                if (String.IsNullOrEmpty(testCSharpScriptReturnStatement.SemicolonToken.Text))
                {
                    var returnVariableName = (testCSharpScriptReturnStatement.Expression as IdentifierNameSyntax).Identifier.Text;
                    if (returnVariableName != null)
                    {

                        var newNode = SyntaxFactory.ParseStatement($"return (\"{returnVariableName}\",{returnVariableName});");
                        var newTree2 = root.ReplaceNode(testCSharpScriptReturnStatement, newNode);

                        return newTree2;

                    }

                }
            }

            //When we just want to execute statements we return null
            var newReturnStatement = SyntaxFactory.ParseStatement("return (null,null);");

            var newTree = root.InsertNodesAfter(lastNode, new List<SyntaxNode>() { newReturnStatement });

            return newTree;


        }
    }
}
