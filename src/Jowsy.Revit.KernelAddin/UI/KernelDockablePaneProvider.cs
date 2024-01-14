using Autodesk.Revit.UI;
using Jowsy.Revit.KernelAddin.UI;

namespace Jowsy.Revit.KernelAddin.UI
{
    internal class KernelDockablePaneProvider : IDockablePaneProvider
    {
        private KernelViewer _dockableWindow;
        internal static ViewModel _viewModel;


        public KernelDockablePaneProvider(ViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        /// <summary>
        /// Builds the dockable pane
        /// </summary>
        /// <param name="data"></param>
        public void SetupDockablePane(DockablePaneProviderData data)
        {

            // Create a new instance of the dockable window
            _dockableWindow = new KernelViewer();
            _dockableWindow.DataContext = _viewModel;
            _dockableWindow.Title = ".NET Interactive Revit Kernel";

            // Connect to the providerData
            data.FrameworkElement = _dockableWindow;
            data.InitialState = new DockablePaneState();
            data.InitialState.DockPosition = DockPosition.Tabbed;
            data.VisibleByDefault = false;
        }
    }
}
