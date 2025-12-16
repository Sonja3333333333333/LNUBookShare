using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;
using LNUBookShareBLL.Features.Favorites;

using LNUBookShareUI.Common;

using MediatR;

using Microsoft.Extensions.Logging;

namespace LNUBookShareUI.ViewModels
{
    public class FavoritesViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;
        private readonly int _pageSize = 10;
        private readonly ILogger<FavoritesViewModel> _logger;

        private ObservableCollection<FavoriteBookCardDto> _favoriteBooks = new();
        private int _totalResults;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;

        public FavoritesViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession, ILogger<FavoritesViewModel> logger)
        {
            _mediator = mediator;
            _userSession = userSession;
            _navigationService = navigationService;
            _logger = logger;

            SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" },
            };

            GoBackCommand = new RelayCommand<object>(GoBack);
            RemoveFromFavoritesCommand = new RelayCommand<int>(async (bookId) => await RemoveFavoriteAsync(bookId));
            ClearFavoritesCommand = new RelayCommand(async () => await ClearFavoritesAsync());

            SetFilterAllCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.All);
            SetFilterAvailableCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.Available);
            SetFilterIssuedCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.Issued);

            NextPageCommand = new RelayCommand(async () => await GoToNextPageAsync(), CanGoToNextPage);
            PreviousPageCommand = new RelayCommand(async () => await GoToPreviousPageAsync(), CanGoToPreviousPage);

            OpenBookDetailsCommand = new RelayCommand<int>(OpenBookDetails);
            ViewOwnerProfileCommand = new RelayCommand<int>(ViewOwnerProfile);

            _logger.LogInformation("FavoritesViewModel ініціалізовано для користувача ID: {UserId}.", _userSession.GetUserId());

            _ = LoadFavoritesAsync();
        }

        public ObservableCollection<FavoriteBookCardDto> FavoriteBooks
        {
            get => _favoriteBooks;
            set => SetProperty(ref _favoriteBooks, value);
        }

        public int TotalResults
        {
            get => _totalResults;
            set => SetProperty(ref _totalResults, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public Dictionary<BookSortCriteria, string> SortOptions { get; }

        public BookSortCriteria SelectedSort
        {
            get => _selectedSort;
            set
            {
                if (SetProperty(ref _selectedSort, value))
                {
                    _logger.LogInformation("Користувач ID: {UserId} змінив сортування на: {Sort}.", _userSession.GetUserId(), value);
                    _ = LoadFavoritesAsync();
                }
            }
        }

        public BookFilterStatus SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value))
                {
                    _logger.LogInformation("Користувач ID: {UserId} змінив фільтр статусу на: {Filter}.", _userSession.GetUserId(), value);
                    _ = LoadFavoritesAsync();
                }
            }
        }

        public ICommand GoBackCommand { get; }

        public ICommand RemoveFromFavoritesCommand { get; }

        public ICommand ClearFavoritesCommand { get; }

        public ICommand SetFilterAllCommand { get; }

        public ICommand SetFilterAvailableCommand { get; }

        public ICommand SetFilterIssuedCommand { get; }

        public ICommand NextPageCommand { get; }

        public ICommand PreviousPageCommand { get; }

        public ICommand OpenBookDetailsCommand { get; }

        public ICommand ViewOwnerProfileCommand { get; }

        private void OpenBookDetails(int bookId)
        {
            if (bookId > 0)
            {
                _logger.LogInformation("Користувач ID: {UserId} переглядає деталі книги ID: {BookId} зі списку улюблених.", _userSession.GetUserId(), bookId);
                _navigationService.ShowBookDetails(bookId);
            }
        }

        private void ViewOwnerProfile(int ownerId)
        {
            if (ownerId > 0)
            {
                _logger.LogInformation("Користувач ID: {UserId} переглядає профіль власника ID: {OwnerId} зі списку улюблених.", _userSession.GetUserId(), ownerId);
                _navigationService.ShowViewProfile(ownerId);
            }
        }

        private async Task LoadFavoritesAsync()
        {
            IsLoading = true;
            int userId = _userSession.GetUserId();
            _logger.LogInformation(
                "Завантаження улюблених для користувача ID: {UserId}. Сторінка: {Page}, Фільтр: {Filter}, Сортування: {Sort}",
                userId,
                _currentPage,
                _selectedStatusFilter,
                _selectedSort);

            try
            {
                var query = new GetFavoriteBooksQuery
                {
                    CurrentUserId = _userSession.GetUserId(),
                    FilterBy = SelectedStatusFilter,
                    SortBy = SelectedSort,
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                };

                var result = await _mediator.Send(query);

                _totalPages = (int)Math.Ceiling((double)result.TotalCount / _pageSize);
                if (_totalPages == 0)
                {
                    _totalPages = 1;
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    FavoriteBooks.Clear();

                    foreach (var book in result.Items)
                    {
                        FavoriteBooks.Add(book);
                    }

                    TotalResults = result.TotalCount;
                    CommandManager.InvalidateRequerySuggested();
                });

                _logger.LogInformation("Улюблені завантажено для користувача ID: {UserId}. Знайдено {Count} книг.", userId, result.TotalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка завантаження улюблених для користувача ID: {UserId}.", userId);
                _ = MessageBox.Show($"Помилка завантаження вподобаних: {ex.Message}", "Помилка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RemoveFavoriteAsync(int bookId)
        {
            int userId = _userSession.GetUserId();
            _logger.LogInformation("Користувач ID: {UserId} ініціював видалення книги ID: {BookId} з улюблених.", userId, bookId);

            try
            {
                var command = new ToggleFavoriteCommand
                {
                    BookId = bookId,
                    UserId = _userSession.GetUserId(),
                };

                _ = await _mediator.Send(command);

                _logger.LogInformation("Книгу ID: {BookId} успішно видалено зі списку улюблених користувача ID: {UserId}.", bookId, userId);
                await LoadFavoritesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при видаленні книги ID: {BookId} з улюблених для користувача ID: {UserId}.", bookId, userId);
                _ = MessageBox.Show($"Помилка видалення з вподобаних: {ex.Message}", "Помилка");
            }
        }

        private async Task ClearFavoritesAsync()
        {
            IsLoading = true;
            int userId = _userSession.GetUserId();

            // Логуємо намір
            _logger.LogWarning("Користувач ID: {UserId} натиснув кнопку 'Очистити все'. Очікування підтвердження.", userId);

            try
            {
                var result = MessageBox.Show(
                    "Ви впевнені, що хочете видалити ВСІ книги з вподобань?",
                    "Підтвердження",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    _logger.LogInformation("Користувач ID: {UserId} скасував очищення списку улюблених.", userId);
                    return; // Тут ми виходимо, finally спрацює і вимкне лоадер
                }

                _logger.LogInformation("Користувач ID: {UserId} підтвердив очищення. Виконується видалення...", userId);

                var command = new ClearFavoritesCommand { UserId = userId };
                _ = await _mediator.Send(command);

                _logger.LogInformation("Список улюблених для користувача ID: {UserId} успішно очищено.", userId);

                await LoadFavoritesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка очищення списку улюблених для користувача ID: {UserId}.", userId);
                _ = MessageBox.Show($"Помилка очищення: {ex.Message}", "Помилка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanGoToNextPage() => _currentPage < _totalPages;

        private async Task GoToNextPageAsync()
        {
            if (CanGoToNextPage())
            {
                _logger.LogInformation("Користувач ID: {UserId} переходить на наступну сторінку улюблених.", _userSession.GetUserId());
                CurrentPage++;
                await LoadFavoritesAsync();
            }
        }

        private bool CanGoToPreviousPage() => _currentPage > 1;

        private async Task GoToPreviousPageAsync()
        {
            if (CanGoToPreviousPage())
            {
                _logger.LogInformation("Користувач ID: {UserId} переходить на попередню сторінку улюблених.", _userSession.GetUserId());
                CurrentPage--;
                await LoadFavoritesAsync();
            }
        }

        private void GoBack(object parameter)
        {
            _logger.LogInformation("Користувач ID: {UserId} виходить зі списку улюблених.", _userSession.GetUserId());
            if (parameter is Window w)
            {
                w.Close();
            }
        }
    }
}