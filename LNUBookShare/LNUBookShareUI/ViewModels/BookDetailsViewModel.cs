using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Favorites;

using LNUBookShareUI.Common;

using MediatR;

using Microsoft.Extensions.Logging;

namespace LNUBookShareUI.ViewModels
{
    public class BookDetailsViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;
        private readonly ILogger<BookDetailsViewModel> _logger;

        private BookDetailsDto _book = new ();

        public BookDetailsViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession, ILogger<BookDetailsViewModel> logger)
        {
            _mediator = mediator;
            _navigationService = navigationService;
            _userSession = userSession;
            _logger = logger;

            GoBackCommand = new RelayCommand<object>(GoBack);
            ToggleFavoriteCommand = new RelayCommand(async () => await ToggleFavorite());
            ViewOwnerProfileCommand = new RelayCommand(ViewOwnerProfile);

            _logger.LogInformation("BookDetailsViewModel створено.");
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
            IsLoading = true;
            _logger.LogInformation("Запит на завантаження деталей книги ID: {BookId}", bookId);

            try
            {
                Book = await _mediator.Send(new GetBookDetailsQuery
                {
                    BookId = bookId,
                    CurrentUserId = _userSession.GetUserId(),
                });

                if (Book != null)
                {
                    _logger.LogInformation("Деталі книги '{Title}' успішно завантажено.", Book.Title);
                }
                else
                {
                    _logger.LogWarning("Книгу з ID {BookId} не знайдено (повернувся null).", bookId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка завантаження деталей книги ID: {BookId}", bookId);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ViewOwnerProfile()
        {
            if (Book != null && Book.OwnerId > 0)
            {
                _logger.LogInformation("Користувач (UserId: {UserId}) переглядає профіль власника книги (OwnerId: {OwnerId}).", _userSession.GetUserId(), Book.OwnerId);
                _navigationService.ShowViewProfile(Book.OwnerId);
            }
            else
            {
                _logger.LogWarning("Не вдалося відкрити профіль власника: дані книги відсутні або OwnerId некоректний.");
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
                _logger.LogWarning("Спроба додати в улюблене, але книга не ініціалізована.");
                return;
            }

            _logger.LogInformation("Зміна статусу 'Вподобане' для книги ID: {BookId}", Book.BookId);

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
                _logger.LogError(ex, "Помилка оновлення статусу Вподобане для книги ID: {BookId}", Book.BookId);
            }
        }
    }
}