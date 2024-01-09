using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Connection;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;
using Microsoft.DotNet.Interactive.ValueSharing;
using System.IO.Pipes;
using System.Text;

namespace Jo.Interactive.Extensions.Revit
{

    public class RevitKernel : Kernel, 
                               IKernelCommandHandler<SubmitCode>, 
                               IKernelCommandHandler<RequestValue>, 
                               IKernelCommandHandler<SendValue>,
                               IKernelCommandHandler<RequestValueInfos>
    {
        private readonly Dictionary<string, object> _variables = new(StringComparer.InvariantCultureIgnoreCase);

        private ScriptOptions _scriptOptions;
  
         
        public RevitKernel(string name) : base(name)
        {
         

            KernelInfo.LanguageName = "C#";
            KernelInfo.LanguageVersion = "12.0";
            KernelInfo.DisplayName = $"RevitKernel - C# Script";
          
           /* _scriptOptions = ScriptOptions.Default
      .WithLanguageVersion(LanguageVersion.La
           )
      .AddImports(
          "System",
          "System.Text",
          "System.Collections",
          "System.Collections.Generic",
          "System.Threading.Tasks",
          "System.Linq")
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
           */
        }

        public Task HandleAsync(SubmitCode command, KernelInvocationContext context)
        {
            throw new NotImplementedException();
        }

        Task IKernelCommandHandler<RequestValue>.HandleAsync(RequestValue command, KernelInvocationContext context)
        {
            if (_variables.TryGetValue(command.Name, out var value))
            {
                context.PublishValueProduced(command, value);
            }
            else
            {
                context.Fail(command, message: $"Value '{command.Name}' not found in kernel {Name}");
            }

            return Task.CompletedTask;
        }
        /*public Task HandleAsync(RequestValue command, KernelInvocationContext context)
        {
            if (_variables.TryGetValue(command.Name, out var value))
            {
                var valueProduced = new ValueProduced(
                    value,
                    command.Name,
                    CreateSingleFromObject(value, JsonFormatter.MimeType),
                    command);
                context.Publish(valueProduced);
            }
            else
            {
                context.Fail(command, message: $"Value not found: {command.Name}");
            }

            return Task.CompletedTask;
        }*/
        Task IKernelCommandHandler<RequestValueInfos>.HandleAsync(RequestValueInfos command, KernelInvocationContext context)
        {
            var valueInfos = _variables.Select(v => new KernelValueInfo(v.Key, CreateSingleFromObject(v.Value, PlainTextSummaryFormatter.MimeType), v.GetType())).ToArray();

            context.Publish(new ValueInfosProduced(valueInfos, command));

            return Task.CompletedTask;
        }

        public static FormattedValue CreateSingleFromObject(object value, string mimeType = null)
        {
            if (mimeType is null)
            {
                mimeType = Formatter.GetPreferredMimeTypesFor(value?.GetType()).First();
            }

            return new FormattedValue(mimeType, value.ToDisplayString(mimeType));
        }

        public async Task HandleAsync(SendValue command, KernelInvocationContext context)
        {
            await SetValueAsync(command, context, SetValueAsync);
        }

        private Task SetValueAsync(string valueName, object value, Type? declaredType = null)
        {
            _variables[valueName] = value;
            return Task.CompletedTask;
        }

        public Task HandleAsync(RequestValueInfos command, KernelInvocationContext context)
        {
            throw new NotImplementedException();
        }

        /* public Task HandleAsync(SubmitCode command, KernelInvocationContext context)
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
         }*/
    }
}