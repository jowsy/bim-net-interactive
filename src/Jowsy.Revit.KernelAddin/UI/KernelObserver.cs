using Microsoft.DotNet.Interactive.Events;

namespace Jowsy.Revit.KernelAddin.UI
{
    public class KernelObserver : IObserver<KernelEvent>
    {
        public KernelObserver(ViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public ViewModel ViewModel { get; }

        public void OnCompleted()
        {
            // throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            // throw new NotImplementedException();
        }

        public void OnNext()
        {

        }

        public void OnNext(KernelEvent value)
        {
            ViewModel.KernelCommands.Add(new CommandViewItem()
            {
                KernelEvent = value.GetType().Name,
                Command = value.Command.GetType().Name,
                TargetKernel = value.Command.TargetKernelName


            });
        }
    }
}
