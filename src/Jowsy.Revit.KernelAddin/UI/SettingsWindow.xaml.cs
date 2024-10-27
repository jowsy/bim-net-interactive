using Microsoft.DotNet.Interactive.Formatting;
using System.Windows;

namespace Jowsy.Revit.KernelAddin.UI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            //TODO: Use MVVM-pattern

            //This is not how WPF should be used but it is quick :)
            txtRecursionLimit.Text = Formatter.RecursionLimit.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtRecursionLimit.Text, out int recursionLimit))
            {
                MessageBox.Show("Recursion limit must be an integer");
                return;
            }

            Formatter.RecursionLimit = recursionLimit;

            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
