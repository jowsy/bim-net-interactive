using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Jowsy.CSharp
{
    public sealed class UndeclaredVariableRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _model;
        public UndeclaredVariableRewriter(SemanticModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            var nodeʹ = base.VisitIdentifierName(node);
            var symbol = _model.GetSymbolInfo(node);

            // We only care about *undeclared* identifiers...
            if (symbol.Symbol != null) return nodeʹ;
            if (symbol.CandidateSymbols.Any()) return nodeʹ;

            // ... and we don't care about unknown method invokes either.
            var parent = nodeʹ.Parent;
            if (parent.IsKind(SyntaxKind.InvocationExpression))
                return nodeʹ;

            return CreateResolverSyntax(node.Identifier);
        }
        private SyntaxNode CreateResolverSyntax(SyntaxToken token)
        {
            var literal = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(token.ToString()));
            var indexSyntax = BracketedArgumentList(SingletonSeparatedList(Argument(literal)));
            var resolveSyntax = ElementAccessExpression(IdentifierName("_resolver"), indexSyntax);
            return resolveSyntax;
        }
    }
}
