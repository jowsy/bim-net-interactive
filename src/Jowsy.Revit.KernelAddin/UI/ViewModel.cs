using Autodesk.Revit.UI;
using Jowsy.Revit.KernelAddin.Core;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Connection;
using System.Collections.ObjectModel;
using System.IO.Pipes;
using System.Reactive.Linq;
using System.Runtime.Remoting.Contexts;

namespace Jowsy.Revit.KernelAddin.UI
{
    public class ViewModel : ViewModelBase, IDisposable
    {
        public RelayCommand StartCommand { get; }
        public RelayCommand RestartCommand { get; }
        private const string NamedPipeName = "revit-kernel-2024-pipe";
        private readonly UIApplication _uiApp;
        private ObservableCollection<CommandViewItem> _kernelCommands = new ObservableCollection<CommandViewItem>();
        private Variables _variablesStore;
        private CompositeKernel _kernel;

        private KernelStatus _kernelStatus;

        public KernelStatus KernelStatus
        {
            get { return _kernelStatus; }
            set { _kernelStatus = value;
                OnPropertyChanged(nameof(KernelStatus));
                OnPropertyChanged(nameof(StartButtonEnabled));
            }
        }

        public bool StartButtonEnabled => KernelStatus == KernelStatus.Stopped;

        public ObservableCollection<CommandViewItem> KernelCommands
        {
            get { return _kernelCommands; }
            set
            {
                _kernelCommands = value;
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


        public ViewModel()
        {
            
            _variablesStore = new Variables();
            _variablesStore.VariablesChanged += _variablesStore_VariablesChanged;

            StartCommand = new RelayCommand(async (c) =>
            {
                await InitKernel();
            });


            RestartCommand = new RelayCommand(async (c) =>
            {
                DisposeKernel();
                await InitKernel();
            });
    
        }
        public void DisposeKernel()
        {
            _kernel.Dispose();

            _variablesStore.Clear();

            Variables?.Clear();

            KernelCommands?.Clear();
        }

        public async Task InitKernel()
        {

            _kernel = new CompositeKernel();

            var str = Microsoft.DotNet.Interactive.Formatting.Formatter.DefaultMimeType;

            _kernel.KernelEvents.ObserveOn(SynchronizationContext.Current).Subscribe(new KernelObserver(this), new CancellationToken());

            var revitKernel = new RevitKernel("RevitKernel", _variablesStore);

            _kernel.Add(revitKernel);

            revitKernel.UseValueSharing();

            SetUpNamedPipeKernelConnection();

            KernelStatus = KernelStatus.AwaitingConnection;
        }

        private async Task SetupInitialVariables()
        {
            App.KernelInitEventHandler.Tcs = new TaskCompletionSource<bool>();

            App.InitKernelEvent.Raise();
            
            await App.KernelInitEventHandler.Tcs.Task;

            var uiApp = App.KernelInitEventHandler.UIApplication;
             _variablesStore.Add("uiapp", uiApp);
            _variablesStore.Add("uidoc", uiApp.ActiveUIDocument);
            _variablesStore.Add("doc", uiApp.ActiveUIDocument.Document);

        }

        private void _variablesStore_VariablesChanged(object sender, EventArgs e)
        {
            Variables = new ObservableCollection<VariableViewItem>(_variablesStore.GetVariables()
                                                                                  .Select(v => new VariableViewItem()
                                                                                  {
                                                                                      Name = v.Key,
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

            var host = _kernel.UseHost(sender, receiver, new Uri("kernel://revit-kernel-2024"));

            _kernel.RegisterForDisposal(host);
            _kernel.RegisterForDisposal(receiver);
            _kernel.RegisterForDisposal(serverStream);

            var _ = Task.Run(async () =>
            {
                // required as waiting connection on named pipe server will block
                serverStream.WaitForConnection();
                var _host = host.ConnectAsync();
                await SetupInitialVariables();
                KernelStatus = KernelStatus.Connected;
               
            });
        }

        public void Dispose()
        {
            _kernel.Dispose();
        }
    }

}
