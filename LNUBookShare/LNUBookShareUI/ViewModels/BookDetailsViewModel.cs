using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Favorites;

using LNUBookShareUI.Common;

using MediatR;

namespace LNUBookShareUI.ViewModels
{
    public class BookDetailsViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;
        private BookDetailsDto _book = new ();

        public BookDetailsViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession)
        {
            _mediator = mediator;
            _navigationService = navigationService;
            _userSession = userSession;

            GoBackCommand = new RelayCommand<object>(GoBack);
            ToggleFavoriteCommand = new RelayCommand(async () => await ToggleFavorite());
            ViewOwnerProfileCommand = new RelayCommand(ViewOwnerProfile);
        }

        public BookDetailsDto Book
        {
            get => _book;
            set => SetProperty(ref _book, value);
        }

        public ICommand GoBackCommand { get; }

        public ICommand ToggleFavoriteCommand { get; }

        public ICommand ViewOwnerProfileCommand { get; }

        public async Task LoadBookDetailsAsync(int bookId)
        {
            try
            {
                Book = await _mediator.Send(new GetBookDetailsQuery
                {
                    BookId = bookId,
                    CurrentUserId = _userSession.GetUserId(),
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка завантаження деталей книги: {ex.Message}");
            }
        }

        private void ViewOwnerProfile()
        {
            if (Book != null && Book.OwnerId > 0)
            {
                _navigationService.ShowViewProfile(Book.OwnerId);
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
            if (Book == null || Book.BookId == 0)
            {
                return;
            }

            try
            {
                var command = new ToggleFavoriteCommand
                {
                    BookId = Book.BookId,
                    UserId = _userSession.GetUserId(),
                };

                _ = await _mediator.Send(command);
                await LoadBookDetailsAsync(Book.BookId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка оновлення статусу Вподобане: {ex.Message}");
            }
        }
    }
}