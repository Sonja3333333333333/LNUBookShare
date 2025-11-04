using LNUBookShareUI.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows;

namespace LNUBookShareUI.Views 
{
    public partial class BookDetailsView : Window
    {
        private readonly BookDetailsViewModel _viewModel;

        public BookDetailsView(BookDetailsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
        public async Task LoadBook(int bookId)
        {
            await _viewModel.LoadBookDetailsAsync(bookId);
        }
    }
}