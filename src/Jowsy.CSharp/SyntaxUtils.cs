using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jowsy.CSharp
{
    public class SyntaxUtils
    {
        public static CompilationUnitSyntax GenerateCodeCommand(string script)
        {
            string source = @"using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using BIMKernels.Addins.Revit.Core;

namespace CodeNamespace
{
    public class Command : ICodeCommand
    {
        public event EventHandler<DisplayEventArgs> OnDisplay;

        public (string, object) Execute(UIApplication uiapp)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;"
+ script + @"
        }

        public void display(object o)
        {
            OnDisplay?.Invoke(this, new DisplayEventArgs(o));
        }
    }
}";

            /* string source = @"using System;
             using Autodesk.Revit.Attributes;
             using Autodesk.Revit.DB;
             using Autodesk.Revit.UI;
             using System.Collections;
             using System.Linq;
             using System.Collections.Generic;
             using BIMKernels.Addins.Revit.Core;

             namespace CodeNamespace 
             {
               public class Command : ICodeCommand
               {
                 public event EventHandler<DisplayEventArgs> OnDisplay;

                 public (string, object) Execute(UIApplication uiapp)
                 {
                     UIDocument uidoc = uiapp.ActiveUIDocument;"
 + script + @"
                 }

                 public void display(object o)
                 {
                     OnDisplay?.Invoke(this, new DisplayEventArgs(o));
                 }
               }
             }";*/

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
    }
}
