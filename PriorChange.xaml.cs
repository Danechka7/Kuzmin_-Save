using System.Windows;
using System.Windows.Input;

namespace Kuzmin_ГлазкиSave
{
    public partial class PriorChange : Window
    {
        public PriorChange(int maxPriority)
        {
            InitializeComponent();
            TBPriority.Text = maxPriority.ToString();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender,  RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }
    }
}