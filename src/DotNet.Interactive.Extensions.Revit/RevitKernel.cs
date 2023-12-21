using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Formatting;
using System.IO.Pipes;
using System.Text;

namespace DotNet.Interactive.Extensions.Revit
{

    [Obsolete("Not used. Only for experimentation.")]
    public class RevitKernel : Kernel, IKernelCommandHandler<SubmitCode>, IKernelCommandHandler<RequestValue>, IKernelCommandHandler<SendValue>
    {
        private ScriptOptions _scriptOptions;
  
        public RevitKernel(string name) : base(name)
        {
            KernelInfo.LanguageName = "C#";
            KernelInfo.LanguageVersion = "12.0";
            KernelInfo.DisplayName = $"RevitKernel - C# Script";
            
            _scriptOptions = ScriptOptions.Default
      .WithLanguageVersion(LanguageVersion.Latest)
      .AddImports(
          "System",
          "System.Text",
          "System.Collections",
          "System.Collections.Generic",
          "System.Threading.Tasks",
          "System.Linq",
          "Autodesk.Revit.DB")
      .AddReferences(
          typeof(Enumerable).Assembly,
          typeof(IEnumerable<>).Assembly,
          typeof(Task<>).Assembly,
          typeof(Kernel).Assembly,
          typeof(CSharpKernel).Assembly,
          typeof(PocketView).Assembly);

            RegisterForDisposal(() =>
            {
                _scriptOptions = null;
            });
        }

        public Task HandleAsync(SubmitCode command, KernelInvocationContext context)
        {
            using (var pipe = new NamedPipeClientStream("localhost", "revitdispatcher", PipeDirection.InOut))
            {
                pipe.Connect(5000);
                pipe.ReadMode = PipeTransmissionMode.Message;

                    var input = command.Code;
                    byte[] bytes = Encoding.Default.GetBytes(input);
                    pipe.Write(bytes, 0, bytes.Length);
                    if (input.ToLower() == "exit") return Task.CompletedTask;
                    var result = ReadMessage(pipe);
                context.DisplayStandardOut(Encoding.UTF8.GetString(result));
            }

            return Task.CompletedTask;
        }

        private static byte[] ReadMessage(PipeStream pipe)
        {
            byte[] buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                do
                {
                    var readBytes = pipe.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, readBytes);
                }
                while (!pipe.IsMessageComplete);

                return ms.ToArray();
            }
        }

        public Task HandleAsync(RequestValue command, KernelInvocationContext context)
        {
            context.PublishValueProduced(command, "test");
            return Task.CompletedTask;

        }

        public Task HandleAsync(SendValue command, KernelInvocationContext context)
        {
            throw new NotImplementedException();
        }
    }
}