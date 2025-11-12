using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input; // <-- Додано
using MediatR;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Favorites;
using LNUBookShareUI.Common; // <-- Додано

namespace LNUBookShareUI.ViewModels
{
    // 1. Базовий клас змінено на ViewModelBase
    // 2. 'partial' видалено, оскільки генератори коду більше не потрібні
    public class BookDetailsViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;

        // 3. [ObservableProperty] замінено на стандартну властивість з SetProperty
        private BookDetailsDto _book = new();
        public BookDetailsDto Book
        {
            get => _book;
            set => SetProperty(ref _book, value);
        }

        // 4. Додано властивості ICommand
        public ICommand GoBackCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }

        public BookDetailsViewModel(IMediator mediator)
        {
            _mediator = mediator;

            // 5. Команди ініціалізовано в конструкторі
            GoBackCommand = new RelayCommand<object>(GoBack);
            ToggleFavoriteCommand = new RelayCommand(async () => await ToggleFavorite());
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