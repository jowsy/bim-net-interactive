using DotNet.Interactive.Extensions.Revit;
using FluentAssertions;

//using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using System;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reactive.Linq;

//using Microsoft.DotNet.Interactive.Tests.Utility;
using Xunit;

namespace RevitExtensionTests { 

    public class RevitExtensionTest : IDisposable
    {
        private Process process;
        private readonly Kernel _kernel;
   
        public RevitExtensionTest()
        {
            _kernel = new CompositeKernel
            {
                new CSharpKernel()
            };

            var extension = new CSharpKernelExtension();

            extension.OnLoadAsync(_kernel.FindKernelByName("csharp"));

            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "C:\\git\\interactive-revitkernel\\interactive-revitkernel\\DispatcherConsole\\bin\\Debug\\DispatcherConsole.exe",
                    UseShellExecute = true,
                    CreateNoWindow = false
                }
            };
            process.Start();
        }

        [Fact]
        public async Task CSharpKernel_sends_code_to_dispatcher()
        {
            bool dirtyFlag = false;
            string codeToSubmit = @"
#!revit2023
int e = 10;";

            var result = await _kernel.SubmitCodeAsync(codeToSubmit);

            var events = result.KernelEvents.Any(e => e is CommandSucceeded);
            var r = await events;
            r.Should().Be(true);

        }

        // teardown
        [Fact]
        public void Dispose()
        {
            process.CloseMainWindow();
            GC.SuppressFinalize(this);
        }
    }
}