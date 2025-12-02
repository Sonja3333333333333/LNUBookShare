using System.Threading.Tasks;
using System.Windows;

using LNUBookShareUI.ViewModels;

namespace LNUBookShareUI.Views
{
    public partial class BookDetailsView : Window
    {
        private readonly BookDetailsViewModel _viewModel;

        public BookDetailsView(BookDetailsViewModel viewModel)
        {
            this.InitializeComponent();
            this._viewModel = viewModel;
            this.DataContext = this._viewModel;
        }

        public async Task LoadBook(int bookId)
        {
            await this._viewModel.LoadBookDetailsAsync(bookId);
        }
    }
}