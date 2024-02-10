using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Elfie.Serialization;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.ValueSharing;
using Microsoft.DotNet.Interactive.Connection;

namespace Jowsy.CSharp.Tests
{
    public class CodeRewriterTests : TestBase
    {
        [Fact]
        public void ItShouldBePossibleToUseImplicitReturn()
        {
            var compilationUnit = SyntaxUtils.GenerateCodeCommand(@"string test = ""hello world"";
                                                                    return test;");

            var actual = StripUsings(SyntaxUtils.FixReturn(compilationUnit)
                                    .NormalizeWhitespace()
                                    .ToFullString());

            string expected = @"namespace Jowsy.Revit.KernelAddin.Core
{
    public class Command : ICodeCommand
    {
        public event EventHandler<DisplayEventArgs> OnDisplay;
        public (string, object) Execute(UIApplication uiapp, Variables __variables)
        {
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
        public void UndeclaredVariablesShouldBeResolvedIfTheyExistGloballyInKernel()
        {
            var valueInfos = new List<KernelValueInfo>(){
                                    new KernelValueInfo("message", 
                                                        new Microsoft.DotNet.Interactive.FormattedValue("text",""),
                                                        typeof(string)),
                                    new KernelValueInfo("number",
                                                        new Microsoft.DotNet.Interactive.FormattedValue("text","1"),
                                                        typeof(int))
                                };


            var sourceText = SyntaxUtils.BuildClassCode(@"message = ""hello world"";
                                                             int result = number + 12;
                                                             return null;");

            var compilation = Compile(sourceText);

            var node = SyntaxUtils.ResolveUndeclaredVariables(compilation, valueInfos);
            var actual = StripUsings(node?.NormalizeWhitespace().ToFullString());
            Debug.Write(actual);

            string expected = @"namespace Jowsy.Revit.KernelAddin.Core
{
    public class Command : ICodeCommand
    {
        public event EventHandler<DisplayEventArgs> OnDisplay;
        public (string, object) Execute(UIApplication uiapp, Variables __variables)
        {
            System.String message = (System.String)__variables.GetVariables()[""message""];
            System.Int32 number = (System.Int32)__variables.GetVariables()[""number""];
            message = ""hello world"";
            int result = number + 12;
            return null;
        }

        public void display(object o)
        {
            OnDisplay?.Invoke(this, new DisplayEventArgs(o));
        }
    }
}";
            expected.Should().Be(actual);
        }

        [Fact]
        public void UndeclaredListVariablesShouldBeResolvedIfTheyExistGloballyInKernel()
        {
            var valueInfos = new List<KernelValueInfo>(){
                                    new KernelValueInfo("numbers",
                                                        new Microsoft.DotNet.Interactive.FormattedValue("text","List"),
                                                        typeof(List<Int32>))
                                };


            var sourceText = SyntaxUtils.BuildClassCode(@"int count = numbers.Count();
                                                             return null;");

            var compilation = Compile(sourceText);

            var node = SyntaxUtils.ResolveUndeclaredVariables(compilation, valueInfos);
            var actual = StripUsings(node?.NormalizeWhitespace().ToFullString());
            Debug.Write(actual);

            string expected = @"namespace Jowsy.Revit.KernelAddin.Core
{
    public class Command : ICodeCommand
    {
        public event EventHandler<DisplayEventArgs> OnDisplay;
        public (string, object) Execute(UIApplication uiapp, Variables __variables)
        {
            System.Collections.Generic.List<System.Int32> numbers = (System.Collections.Generic.List<System.Int32>)__variables.GetVariables()[""numbers""];
            int count = numbers.Count();
            return null;
        }

        public void display(object o)
        {
            OnDisplay?.Invoke(this, new DisplayEventArgs(o));
        }
    }
}";
            expected.Should().Be(actual);
        }

        [Fact]
        public async Task TryCompile()
        {
                RoslynCompilerService service = new RoslynCompilerService("2024");

            var results = await service.CompileRevitAddin("height = 5;", false, () =>
            {
                return Task.FromResult(new KernelValueInfo[]{
                                    new KernelValueInfo("height",
                                                        new Microsoft.DotNet.Interactive.FormattedValue("text","5"),
                                                        typeof(int)),
                                });

            });



        }
       
        public static string StripUsings(string code)
        {
            var index = code.ToLower().IndexOf("namespace");  

            return code.Substring(index);
        }
    }
}