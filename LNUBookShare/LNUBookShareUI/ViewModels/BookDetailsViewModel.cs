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

        private BookDetailsDto _book = new();
        public BookDetailsDto Book
        {
            get => _book;
            set => SetProperty(ref _book, value);
        }

        
        public ICommand GoBackCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }

        public ICommand ViewOwnerProfileCommand { get; }

        public BookDetailsViewModel(IMediator mediator, INavigationService navigationService)
        {
            _mediator = mediator;
            _navigationService = navigationService;

            GoBackCommand = new RelayCommand<object>(GoBack);
            ToggleFavoriteCommand = new RelayCommand(async () => await ToggleFavorite());
            ViewOwnerProfileCommand = new RelayCommand(ViewOwnerProfile);
        }

        private void ViewOwnerProfile()
        {
            
            if (Book != null && Book.OwnerId > 0)
            {
                _navigationService.ShowViewProfile(Book.OwnerId);
            }
        }

        public async Task LoadBookDetailsAsync(int bookId)
        {
            int currentUserId = 1;
            try
            {
                Book = await _mediator.Send(new GetBookDetailsQuery
                {
                    BookId = bookId,
                    CurrentUserId = currentUserId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка завантаження деталей книги: {ex.Message}");
                // TODO: Показати MessageBox
            }
        }

        // 6. Атрибут [RelayCommand] видалено
        private void GoBack(object window)
        {
            if (window is Window w)
            {
                w.Close();
            }
        }

        // 7. Атрибут [RelayCommand] видалено
        private async Task ToggleFavorite()
        {
            if (Book == null || Book.BookId == 0) return;

            int currentUserId = 1;

            try
            {
                var command = new ToggleFavoriteCommand
                {
                    BookId = Book.BookId,
                    UserId = currentUserId
                };

                await _mediator.Send(command);
                await LoadBookDetailsAsync(Book.BookId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка оновлення статусу Вподобане: {ex.Message}");
                // TODO: Показати MessageBox
            }
        }
    }
}