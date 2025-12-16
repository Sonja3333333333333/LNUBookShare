using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Favorites;

using LNUBookShareUI.Common;

using MediatR;

using Microsoft.Extensions.Logging;

namespace LNUBookShareUI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;

        private readonly ILogger<MainViewModel> _logger;

        private readonly int _pageSize = 10;

        private bool _isSearchPerformed = false;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private ObservableCollection<BookCardDto> _books = new ();
        private int _totalResults;
        private BookSearchCriteria _selectedSearchCriteria = BookSearchCriteria.Title;
        private string _searchTerm = string.Empty;
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;

        public MainViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession, ILogger<MainViewModel> logger)
        {
            _mediator = mediator;
            _navigationService = navigationService;
            _userSession = userSession;
            _logger = logger;

            SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" },
                { BookSortCriteria.Category, "Категорія" },
            };

            SearchOptions = new Dictionary<BookSearchCriteria, string>
            {
                { BookSearchCriteria.Title, "Назва" },
                { BookSearchCriteria.Author, "Автор" },
                { BookSearchCriteria.ISBN, "ISBN" },
                { BookSearchCriteria.Category, "Категорія" },
            };

            LoadBooksCommand = new RelayCommand(async () => await SearchAsync());
            ToggleFavoriteCommand = new RelayCommand<int>(async (id) => await ToggleFavoriteAsync(id));

            SetFilterAllCommand = new RelayCommand(() => SetFilter(BookFilterStatus.All));
            SetFilterAvailableCommand = new RelayCommand(() => SetFilter(BookFilterStatus.Available));
            SetFilterIssuedCommand = new RelayCommand(() => SetFilter(BookFilterStatus.Issued));

            NextPageCommand = new RelayCommand(async () => await GoToNextPageAsync(), CanGoToNextPage);
            PreviousPageCommand = new RelayCommand(async () => await GoToPreviousPageAsync(), CanGoToPreviousPage);

            OpenProfileCommand = new RelayCommand(OpenProfile);
            OpenFavoritesCommand = new RelayCommand(OpenFavorites);
            ViewOwnerProfileCommand = new RelayCommand<int>(ViewOwnerProfile);
            OpenBookDetailsCommand = new RelayCommand<int>(OpenBookDetails);

            _logger.LogInformation("MainViewModel ініціалізовано для користувача ID: {UserId}.", _userSession.GetUserId());

            _ = LoadBooksAsync();
        }

        public ObservableCollection<BookCardDto> Books
        {
            get => _books;
            set => SetProperty(ref _books, value);
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

        public Dictionary<BookSearchCriteria, string> SearchOptions { get; }

        public BookSearchCriteria SelectedSearchCriteria
        {
            get => _selectedSearchCriteria;
            set => SetProperty(ref _selectedSearchCriteria, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        public Dictionary<BookSortCriteria, string> SortOptions { get; }

        public BookSortCriteria SelectedSort
        {
            get => _selectedSort;
            set
            {
                bool valueChanged = SetProperty(ref _selectedSort, value);
                if (valueChanged && _isSearchPerformed)
                {
                    _logger.LogInformation("Користувач ID: {UserId} змінив сортування на: {Sort}.", _userSession.GetUserId(), value);
                    CurrentPage = 1;
                    _ = LoadBooksAsync();
                }
            }
        }

        public BookFilterStatus SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value) && _isSearchPerformed)
                {
                    _logger.LogInformation("Користувач ID: {UserId} змінив фільтр статусу на: {Filter}.", _userSession.GetUserId(), value);
                    CurrentPage = 1;
                    _ = LoadBooksAsync();
                }
            }
        }

        public ICommand LoadBooksCommand { get; }

        public ICommand ToggleFavoriteCommand { get; }

        public ICommand SetFilterAllCommand { get; }

        public ICommand SetFilterAvailableCommand { get; }

        public ICommand SetFilterIssuedCommand { get; }

        public ICommand NextPageCommand { get; }

        public ICommand PreviousPageCommand { get; }

        public ICommand OpenProfileCommand { get; }

        public ICommand ViewOwnerProfileCommand { get; }

        public ICommand OpenFavoritesCommand { get; }

        public ICommand OpenBookDetailsCommand { get; }

        private void OpenProfile()
        {
            _logger.LogInformation("Користувач ID: {UserId} переходить у свій профіль.", _userSession.GetUserId());
            _navigationService.ShowProfile();
        }

        private void ViewOwnerProfile(int ownerId)
        {
            _logger.LogInformation("Користувач ID: {UserId} переглядає профіль власника книги ID: {OwnerId}.", _userSession.GetUserId(), ownerId);
            _navigationService.ShowViewProfile(ownerId);
        }

        private void OpenFavorites()
        {
            _logger.LogInformation("Користувач ID: {UserId} переходить у список улюблених.", _userSession.GetUserId());
            _navigationService.ShowFavorites();
        }

        private void SetFilter(BookFilterStatus status)
        {
            if (!_isSearchPerformed)
            {
                _logger.LogInformation("Користувач ID: {UserId} встановив фільтр статусу (до пошуку): {Filter}.", _userSession.GetUserId(), status);
            }

            SelectedStatusFilter = status;
        }

        private async Task SearchAsync()
        {
            _logger.LogInformation(
                "Користувач ID: {UserId} виконує пошук. Запит: '{Term}', Критерій: {Criteria}.",
                _userSession.GetUserId(),
                SearchTerm,
                SelectedSearchCriteria);

            CurrentPage = 1;
            await LoadBooksAsync();
        }

        private void OpenBookDetails(int bookId)
        {
            if (bookId > 0)
            {
                _logger.LogInformation("Користувач ID: {UserId} відкриває деталі книги ID: {BookId}.", _userSession.GetUserId(), bookId);
                _navigationService.ShowBookDetails(bookId);
            }
        }

        private async Task LoadBooksAsync()
        {
            IsLoading = true;
            int userId = _userSession.GetUserId();

            try
            {
                var query = new GetBooksQuery
                {
                    CurrentUserId = _userSession.GetUserId(),
                    SearchTerm = SearchTerm,
                    SearchBy = SelectedSearchCriteria,
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    FilterBy = SelectedStatusFilter,
                    SortBy = SelectedSort,
                };

                if (string.IsNullOrWhiteSpace(SearchTerm))
                {
                    query.RecommendForUser = true;
                    _logger.LogInformation("Завантаження рекомендованих книг для користувача ID: {UserId}. Сторінка: {Page}.", userId, _currentPage);
                }
                else
                {
                    _logger.LogInformation("Завантаження результатів пошуку для користувача ID: {UserId}. Сторінка: {Page}.", userId, _currentPage);
                }

                //await Task.Delay(3000);
                var result = await _mediator.Send(query);

                _isSearchPerformed = true;

                _totalPages = (int)Math.Ceiling((double)result.TotalCount / _pageSize);
                if (_totalPages == 0)
                {
                    _totalPages = 1;
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    Books.Clear();
                    foreach (var book in result.Items)
                    {
                        Books.Add(book);
                    }

                    TotalResults = result.TotalCount;
                    CommandManager.InvalidateRequerySuggested();
                });

                _logger.LogInformation("Книги завантажено для користувача ID: {UserId}. Знайдено: {Count}.", userId, result.TotalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка завантаження книг для користувача ID: {UserId}.", userId);
                _ = MessageBox.Show($"Сталася помилка: {ex.Message}", "Помилка завантаження");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ToggleFavoriteAsync(int bookId)
        {
            int userId = _userSession.GetUserId();
            _logger.LogInformation("Користувач ID: {UserId} змінює статус 'Вподобане' для книги ID: {BookId}.", userId, bookId);

            try
            {
                var command = new ToggleFavoriteCommand
                {
                    BookId = bookId,
                    UserId = userId,
                };

                _ = await _mediator.Send(command);
                await LoadBooksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при зміні статусу 'Вподобане' книги ID: {BookId} для користувача ID: {UserId}.", bookId, userId);

                string errorMessage = $"Помилка: {ex.Message}\n\n" +
                                      $"Деталі (InnerException): {ex.InnerException?.Message}";

                _ = MessageBox.Show(errorMessage, "Помилка (ToggleFavorite)");
            }
        }

        private bool CanGoToNextPage() => _isSearchPerformed && _currentPage < _totalPages;

        private async Task GoToNextPageAsync()
        {
            if (CanGoToNextPage())
            {
                _logger.LogInformation("Користувач ID: {UserId} переходить на наступну сторінку пошуку/рекомендацій.", _userSession.GetUserId());
                CurrentPage++;
                await LoadBooksAsync();
            }
        }

        private bool CanGoToPreviousPage() => _isSearchPerformed && _currentPage > 1;

        private async Task GoToPreviousPageAsync()
        {
            if (CanGoToPreviousPage())
            {
                _logger.LogInformation("Користувач ID: {UserId} переходить на попередню сторінку пошуку/рекомендацій.", _userSession.GetUserId());
                CurrentPage--;
                await LoadBooksAsync();
            }
        }
    }
}