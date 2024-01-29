using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Jowsy.Revit.KernelAddin;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Connection;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;
using Microsoft.DotNet.Interactive.ValueSharing;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Jowsy.Revit.KernelAddin.Core
{

    public class RevitKernel : Kernel,
                               IKernelCommandHandler<SubmitCode>,
                               IKernelCommandHandler<RequestValue>,
                               IKernelCommandHandler<SendValue>,
                               IKernelCommandHandler<RequestValueInfos>
    {
        private readonly Variables _variablesStore;
        //private ScriptOptions _scriptOptions;


        public RevitKernel(string name, Variables variablesStore) : base(name)
        {


            KernelInfo.LanguageName = "C#";
            KernelInfo.LanguageVersion = "7.3"; //All .NET framework
            KernelInfo.DisplayName = $"RevitKernel - C# Script";
            _variablesStore = variablesStore ?? throw new ArgumentNullException(nameof(variablesStore));

        }

        public async Task HandleAsync(SubmitCode command, KernelInvocationContext context)
        {
            App.KernelEventHandler.tcs = new TaskCompletionSource<(string, object)>();
            App.KernelEventHandler.KernelContext = context;
            App.KernelEventHandler.Variables = _variablesStore;

            _variablesStore.TryGetValue("assemblyPath", out string dllPath);

            if (string.IsNullOrEmpty(dllPath))
            {
                context.Fail(command);
            }

            App.KernelEventHandler.CompiledDllPath = dllPath;
            App.KernelEvent.Raise();
            var result = await App.KernelEventHandler.tcs.Task;
            if (result != (null, null))
            {
                _variablesStore.Add(result.Item1, result.Item2);
            }
            
            context.Complete(command);
        }

        public Task HandleAsync(RequestValue command, KernelInvocationContext context)
        {
            if (_variablesStore.TryGetValue(command.Name, out var value))
            {
                context.PublishValueProduced(command, value);
            }
            else
            {
                context.Fail(command, message: $"Value '{command.Name}' not found in kernel {Name}");
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(RequestValueInfos command, KernelInvocationContext context)
        {
            var valueInfos = _variablesStore.GetVariables()
                                            .Select(v => 
                                                    new KernelValueInfo(v.Key,                         
                                                    CreateSingleFromObject(v.Value, PlainTextSummaryFormatter.MimeType), 
                                                    v.Value.GetType()))
                                            .ToArray();

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

        private Task SetValueAsync(string valueName, object value, Type declaredType = null)
        {
            _variablesStore.Add(valueName, value);
            return Task.CompletedTask;
        }

        /*public async Task HandleAsync(RequestHoverText command, KernelInvocationContext context)
        {
            //var text = await document.GetTextAsync(context.CancellationToken);
            var code = command.Code;

            context.Publish(
                new HoverTextProduced(
                    command,
                    new[]
                    {
                        new FormattedValue("text/markdown", "hej")
                    }));
        }
        */

    }
}