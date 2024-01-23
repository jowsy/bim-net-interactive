using System.ComponentModel;

namespace Jowsy.Revit.KernelAddin.UI
{
    public enum KernelStatus
    {
        [Description(nameof(Stopped))]
        Stopped = 0,
        [Description("Awaiting connection...")]
        AwaitingConnection = 1,
        [Description(nameof(Connected))]
        Connected = 2
    }
}