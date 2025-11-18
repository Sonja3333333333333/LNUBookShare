using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input; 
using MediatR;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Favorites;
using LNUBookShareUI.Common; 

namespace LNUBookShareUI.ViewModels
{
    
    public class BookDetailsViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;

        private readonly INavigationService _navigationService;

        private readonly IUserSession _userSession;

        private BookDetailsDto _book = new();
        public BookDetailsDto Book
        {
            get => this._book;
            set => this.SetProperty(ref this._book, value);
        }

        
        public ICommand GoBackCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }

        public ICommand ViewOwnerProfileCommand { get; }

        public BookDetailsViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession)
        {
            this._mediator = mediator;
            this._navigationService = navigationService;
            this._userSession = userSession;

            this.GoBackCommand = new RelayCommand<object>(this.GoBack);
            this.ToggleFavoriteCommand = new RelayCommand(async () => await this.ToggleFavorite());
            this.ViewOwnerProfileCommand = new RelayCommand(this.ViewOwnerProfile);
        }

        private void ViewOwnerProfile()
        {
            
            if (this.Book != null && this.Book.OwnerId > 0)
            {
                this._navigationService.ShowViewProfile(this.Book.OwnerId);
            }
        }

        public async Task LoadBookDetailsAsync(int bookId)
        {
 
            try
            {
                this.Book = await this._mediator.Send(new GetBookDetailsQuery
                {
                    BookId = bookId,
                    CurrentUserId = this._userSession.GetUserId()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка завантаження деталей книги: {ex.Message}");
            }
        }

        private void GoBack(object window)
        {
            if (window is Window w)
            {
                w.Close();
            }
        }

        private async Task ToggleFavorite()
        {
            if (this.Book == null || this.Book.BookId == 0)
            {
                return;
            }

            try
            {
                var command = new ToggleFavoriteCommand
                {
                    BookId = this.Book.BookId,
                    UserId = this._userSession.GetUserId()
                };

                _ = await this._mediator.Send(command);
                await this.LoadBookDetailsAsync(this.Book.BookId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка оновлення статусу Вподобане: {ex.Message}");
            }
        }
    }
}