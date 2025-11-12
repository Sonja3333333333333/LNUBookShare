using System.Windows;
using System.Windows.Controls;
using LNUBookShareUI.ViewModels;

namespace LNUBookShareUI.Views
{
    public partial class EditBookView : Window
    {
        public EditBookView()
        {
            InitializeComponent();
        }

        private void StatusRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && DataContext is EditBookViewModel vm)
            {
                vm.Status = rb.Tag?.ToString();
            }
        }
    }
}