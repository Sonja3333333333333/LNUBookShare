using System;
using System.Threading.Tasks;
using MediatR;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;     
using LNUBookShareBLL.Features.Favorites; 

namespace LNUBookShareUI.ViewModels 
{
    public partial class BookDetailsViewModel : ObservableObject
    {
        private readonly IMediator _mediator;

        [ObservableProperty]
        private BookDetailsDto _book = new(); 
        public BookDetailsViewModel(IMediator mediator)
        {
            _mediator = mediator;
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
            }
        }

        [RelayCommand]
        private async Task ToggleFavorite()
        {
            if (Book == null || Book.BookId == 0) return;

            int currentUserId = 1; 

            await _mediator.Send(new ToggleFavoriteCommand
            {
                BookId = Book.BookId,
                UserId = currentUserId
            });

            Book.IsFavoritedByCurrentUser = !Book.IsFavoritedByCurrentUser;

            OnPropertyChanged(nameof(Book));
        }
    }
}