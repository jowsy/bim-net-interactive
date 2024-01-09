using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Connection;
using Microsoft.DotNet.Interactive.Events;
using RevitKernel.UI;
using RevitKernelUI.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CommandLine;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace RevitKernelUI
{
    public class ViewModel : ViewModelBase, IDisposable
    {
        public RelayCommand StartCommand { get; }
        public RelayCommand RestartCommand { get; }
        private const string NamedPipeName = "revit-kernel-2024-pipe";
        private ObservableCollection<CommandViewItem> _kernelCommands = new ObservableCollection<CommandViewItem>();
        private Variables _variablesStore;
        private CompositeKernel _kernel;

        public ObservableCollection<CommandViewItem> KernelCommands
        {
            get { return _kernelCommands; }
            set { _kernelCommands = value;
                OnPropertyChanged(nameof(KernelCommands));
            }
        }

        private ObservableCollection<VariableViewItem> _variables = new ObservableCollection<VariableViewItem>();
        public ObservableCollection<VariableViewItem> Variables
        {
            get { return _variables; }
            set
            {
                _variables = value;
                OnPropertyChanged(nameof(Variables));
            }
        }


        public ViewModel() {

            _variablesStore = new Variables();
            _variablesStore.VariablesChanged += _variablesStore_VariablesChanged;

            StartCommand = new RelayCommand((c) =>
            {
                InitKernel();
            });


            RestartCommand = new RelayCommand((c) =>
            {
                DisposeKernel();
            });


        }
        public void DisposeKernel()
        {
            _kernel.Dispose();

        }

        public void InitKernel()
        {
            //AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("netstandard")).ToList();

            _kernel = new CompositeKernel();

            //Microsoft.DotNet.Interactive.Formatting.PocketView pocketView = new Microsoft.DotNet.Interactive.Formatting.PocketView();
            var str = Microsoft.DotNet.Interactive.Formatting.Formatter.DefaultMimeType;
            //Perkele! 
            _kernel.KernelEvents.ObserveOn(SynchronizationContext.Current).Subscribe(new KernelObserver(this), new System.Threading.CancellationToken());
            var revitKernel = new RevitKernel("RevitKernel", _variablesStore);

            _kernel.Add(revitKernel);
            revitKernel.UseValueSharing();

            revitKernel.AddMiddleware(async (KernelCommand command, KernelInvocationContext context, KernelPipelineContinuation next) =>
            {
                /*if (RunOnDispatcher)
                {
                    await Dispatcher.InvokeAsync(async () => await next(command, context));
                }
                else
                {*/

                /*  await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                          KernelCommands.Add(new KernelCommandViewItem()
                          {
                              Name = command.GetType().Name

                          }));*/

                await next(command, context);
                //}
            });
            SetUpNamedPipeKernelConnection();
        }
        private void _variablesStore_VariablesChanged(object sender, EventArgs e)
        {
            Variables = new ObservableCollection<VariableViewItem>(_variablesStore.GetVariables()
                                                                                  .Select(v => new VariableViewItem() 
                                                                                                { Name = v.Key, 
                                                                                                Value = v.Value?.ToString()
                                                                                  }));
        }
        public bool KernelIsRunning()
        {
            return _kernel != null;
        }
        private void SetUpNamedPipeKernelConnection()
        {
            var serverStream = new NamedPipeServerStream(
                NamedPipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous);

            var sender = KernelCommandAndEventSender.FromNamedPipe(
                serverStream,
                new Uri("kernel://remote-control"));
            var receiver = KernelCommandAndEventReceiver.FromNamedPipe(serverStream);

            var host = _kernel.UseHost(sender, receiver, new Uri("kernel://my-wpf-app"));
       
            _kernel.RegisterForDisposal(host);
            _kernel.RegisterForDisposal(receiver);
            _kernel.RegisterForDisposal(serverStream);

            var _ = Task.Run(() =>
            {
                // required as waiting connection on named pipe server will block
                serverStream.WaitForConnection();
                var _host = host.ConnectAsync();

            });
        }

        public void Dispose()
        {
            _kernel.Dispose();
        }
    }

}
