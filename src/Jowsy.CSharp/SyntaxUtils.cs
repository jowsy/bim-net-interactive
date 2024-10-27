using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.Interactive.ValueSharing;

namespace Jowsy.CSharp
{
    public class SyntaxUtils
    {
        public static string BuildClassCode(string script) =>
@"using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.IFC;
using System.Collections;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using System.Collections.Generic;
using Jowsy.Revit.KernelAddin.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Jowsy.Revit.KernelAddin.Core
{
    public class Command : ICodeCommand
    {
        public event EventHandler<DisplayEventArgs> OnDisplay;

        public (string, object) Execute(UIApplication uiapp, Variables __variables)
        {"
+ script + @"
        }

        public void display(object o)
        {
            OnDisplay?.Invoke(this, new DisplayEventArgs(o));
        }
    }
}";

        public static CompilationUnitSyntax GenerateCodeCommand(string script)
        {

            var source = BuildClassCode(script);

            var tree = CSharpSyntaxTree.ParseText(source);

            var root = (CompilationUnitSyntax)tree.GetRoot().NormalizeWhitespace();

            return root;
        }
        public static CompilationUnitSyntax FixReturn(CompilationUnitSyntax root)
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
                if (string.IsNullOrEmpty(testCSharpScriptReturnStatement.SemicolonToken.Text))
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

        public static SyntaxNode? ResolveUndeclaredVariables(Compilation? compilation, IEnumerable<KernelValueInfo>? valueInfos)
        {
            var diagnostics = compilation.GetDiagnostics().Where(d => d.Id == "CS0103");

            List<string> missingVariableDeclarations = new List<string>();
            foreach (var item in diagnostics)
            {
                var sourceSpan = item.Location?.SourceSpan;
                if (sourceSpan == null)
                {
                    continue;
                }

                var variable = item.Location?
                                   .SourceTree?
                                   .ToString()
                                   .Substring(sourceSpan.Value.Start, sourceSpan.Value.Length);
                if (variable != null)
                {
                    missingVariableDeclarations.Add(variable);
                }
            }

            var tree = compilation.SyntaxTrees.FirstOrDefault();

            //var semanticModel = compilation.GetSemanticModel(tree);

            MethodDeclarationSyntax methodDecl = tree.GetRoot()
                                               .DescendantNodes()
                                               .OfType<ClassDeclarationSyntax>()
                                               .First().ChildNodes()
                                               .OfType<MethodDeclarationSyntax>()
                                               .First();

            var block = methodDecl.DescendantNodes()
                                  .OfType<BlockSyntax>()
                                  .FirstOrDefault();

            SyntaxNode node = block.ChildNodes().First();

            List<SyntaxNode> nodesToAppend = new List<SyntaxNode>();
            foreach (var variable in missingVariableDeclarations.Distinct())
            {
                var valueInfo = valueInfos.Where(vi => vi.Name == variable).FirstOrDefault();

                if (valueInfo != null)
                {

                    string typeName = valueInfo.TypeName;
                    //TypeSyntaxFactory.GetTypeSyntax()

                    var valueInfoNode = SyntaxFactory.ParseStatement($"{valueInfo.TypeName} {valueInfo.Name} = ({valueInfo.TypeName})__variables.GetVariables()[\"{valueInfo.Name}\"];");
                    nodesToAppend.Add(valueInfoNode);
                }

            }
            var result = tree.GetRoot()
                             .InsertNodesBefore(node, nodesToAppend);

            return result;
        }
    }
}
