using System.Reflection;

namespace Jowsy.CSharp
{
    public class CompilationResults
    {
        public bool Success { get; set; }

        public string? DiagnosticText { get; set; }

        public string? AssemblyPath { get; set; }

        public Assembly? Assembly { get; set; }

    }
}