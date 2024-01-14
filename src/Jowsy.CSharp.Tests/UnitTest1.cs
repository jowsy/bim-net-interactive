using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Elfie.Serialization;

namespace Jowsy.CSharp.Tests
{
    public class CodeRewriterTests
    {
        [Fact]
        public void ConvertImplicitReturn()
        {
            var compilationUnit = SyntaxUtils.GenerateCodeCommand(@"string test = ""hello world"";
                                                                    return test;");

            var actual = StripUsings(SyntaxUtils.FixReturn(compilationUnit)
                                    .NormalizeWhitespace()
                                    .ToFullString());

            string expected = @"namespace CodeNamespace
{
    public class Command : ICodeCommand
    {
        public event EventHandler<DisplayEventArgs> OnDisplay;
        public (string, object) Execute(UIApplication uiapp)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            string test = ""hello world"";
            return (""test"", test);
        }

        public void display(object o)
        {
            OnDisplay?.Invoke(this, new DisplayEventArgs(o));
        }
    }
}";
           
            actual.Should().Be(expected);
        }

        [Fact]
        public void TryDefineUndeclaredVariables()
        {

            var compilationUnit = SyntaxUtils.GenerateCodeCommand(@"message = ""hello world"";
                                                                    message;");

            var actual = StripUsings(SyntaxUtils.FixReturn(compilationUnit)
                        .NormalizeWhitespace()
                        .ToFullString());

            string expected = @"namespace CodeNamespace
{
    public class Command : ICodeCommand
    {
        public event EventHandler<DisplayEventArgs> OnDisplay;
        public (string, object) Execute(UIApplication uiapp, Variables globals)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
                     
            if (!globals.TryGetValue<string>(""message"", out string message)){
                throw new ArgumentNullException(nameof(message));
            }

            message = ""hello world"";
            return (""message"", message);
        }

        public void display(object o)
        {
            OnDisplay?.Invoke(this, new DisplayEventArgs(o));
        }
    }
}";

        }

        public static string StripUsings(string code)
        {
            var index = code.ToLower().IndexOf("namespace");  

            return code.Substring(index);
        }
    }
}