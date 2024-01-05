using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RevitKernelUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IntPtr mainWindowHandle, UIApplication uiApp)
        {
            InitializeComponent();

            var interop = new WindowInteropHelper(this);
            interop.EnsureHandle();
            interop.Owner = mainWindowHandle;

            DataContext = new ViewModel(uiApp);

            this.Closing += MainWindow_Closing;
;        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (DataContext as ViewModel).Dispose();
        }
    }
}
