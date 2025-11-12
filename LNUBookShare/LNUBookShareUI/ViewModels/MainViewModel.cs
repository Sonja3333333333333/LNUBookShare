using MediatR;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Favorites;
using LNUBookShareUI.Common;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Navigation;

namespace LNUBookShareUI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // --- Поля ---
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly int _currentUserId = 1; // "Захардкодили" ID користувача!
        private bool _isSearchPerformed = false; // "Прапорець" пошуку

        private int _currentPage = 1;
        private int _totalPages = 1;
        private readonly int _pageSize = 10;

        // --- Властивості ---
        private ObservableCollection<BookCardDto> _books = new();
        public ObservableCollection<BookCardDto> Books
        {
            get => _books;
            set => SetProperty(ref _books, value);
        }

        private int _totalResults;
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

        // --- Критерії Пошуку ---
        public Dictionary<BookSearchCriteria, string> SearchOptions { get; }
        private BookSearchCriteria _selectedSearchCriteria = BookSearchCriteria.Title;
        public BookSearchCriteria SelectedSearchCriteria
        {
            get => _selectedSearchCriteria;
            set => SetProperty(ref _selectedSearchCriteria, value);
        }

        private string _searchTerm = "";
        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        // --- Критерії Сортування ---
        public Dictionary<BookSortCriteria, string> SortOptions { get; }
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        public BookSortCriteria SelectedSort
        {
            get => _selectedSort;
            set
            {
                bool valueChanged = SetProperty(ref _selectedSort, value);
                if (valueChanged && _isSearchPerformed)
                {
                    CurrentPage = 1;
                    _ = LoadBooksAsync();
                }
            }
        }

        // --- Критерії Фільтрації ---
        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;
        public BookFilterStatus SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value) && _isSearchPerformed)
                {
                    CurrentPage = 1;
                    _ = LoadBooksAsync();
                }
            }
        }

        // --- Команди ---
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



        // --- Конструктор ---
        public MainViewModel(IMediator mediator, INavigationService navigationService)
        {
            _mediator = mediator;
            _navigationService = navigationService;

            SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" },
                { BookSortCriteria.Category, "Категорія"}
            };

            SearchOptions = new Dictionary<BookSearchCriteria, string>
            {
                { BookSearchCriteria.Title, "Назва" },
                { BookSearchCriteria.Author, "Автор" },
                { BookSearchCriteria.ISBN, "ISBN" },
                { BookSearchCriteria.Category, "Категорія"}

            };

           

            // Зв'язуємо команди
            LoadBooksCommand = new RelayCommand(async () => await SearchAsync());
            ToggleFavoriteCommand = new RelayCommand<int>(async (id) => await ToggleFavoriteAsync(id));

            // Фільтри
            SetFilterAllCommand = new RelayCommand(() => SetFilter(BookFilterStatus.All));
            SetFilterAvailableCommand = new RelayCommand(() => SetFilter(BookFilterStatus.Available));
            SetFilterIssuedCommand = new RelayCommand(() => SetFilter(BookFilterStatus.Issued));

            // Пагінація
            NextPageCommand = new RelayCommand(async () => await GoToNextPageAsync(), CanGoToNextPage);
            PreviousPageCommand = new RelayCommand(async () => await GoToPreviousPageAsync(), CanGoToPreviousPage);

            OpenProfileCommand = new RelayCommand(OpenProfile);
            OpenFavoritesCommand = new RelayCommand(OpenFavorites);

            ViewOwnerProfileCommand = new RelayCommand<int>(ViewOwnerProfile);
        }

        // --- Логіка ---
        private void OpenProfile()
        {
            // Просто викликаємо метод із сервісу
            _navigationService.ShowProfile();
        }
        private void ViewOwnerProfile(int ownerId)
        {

            _navigationService.ShowViewProfile(ownerId);
        }
        private void OpenFavorites()
        {
            // Робимо те саме, що й для профілю
            _navigationService.ShowFavorites();
        }

        private void SetFilter(BookFilterStatus status)
        {
            SelectedStatusFilter = status;
        }

        private async Task SearchAsync()
        {
            CurrentPage = 1; // Скидаємо сторінку при кожному новому пошуку
            await LoadBooksAsync();
        }

        private async Task LoadBooksAsync()
        {
            try
            {
                var query = new GetBooksQuery
                {
                    CurrentUserId = _currentUserId,
                    SearchTerm = this.SearchTerm,
                    SearchBy = this.SelectedSearchCriteria,
                    PageNumber = this._currentPage,
                    PageSize = this._pageSize,
                    FilterBy = this.SelectedStatusFilter,
                    SortBy = this.SelectedSort
                };

                var result = await _mediator.Send(query);

                _isSearchPerformed = true; // "Піднімаємо прапорець"

                _totalPages = (int)Math.Ceiling((double)result.TotalCount / _pageSize);
                if (_totalPages == 0) _totalPages = 1;

                App.Current.Dispatcher.Invoke(() =>
                {
                    Books.Clear();
                    foreach (var book in result.Items)
                    {
                        Books.Add(book);
                    }
                    TotalResults = result.TotalCount;
                    CommandManager.InvalidateRequerySuggested(); // Оновлюємо стан кнопок ← →
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сталася помилка: {ex.Message}", "Помилка завантаження");
            }
        }

        private async Task ToggleFavoriteAsync(int bookId)
        {
            try
            {
                var command = new ToggleFavoriteCommand
                {
                    BookId = bookId,
                    UserId = _currentUserId
                };

                await _mediator.Send(command);
                await LoadBooksAsync();
            }
            catch (Exception ex)
            {
                // Формуємо деталізоване повідомлення
                string errorMessage = $"Помилка: {ex.Message}\n\n" +
                                      $"Деталі (InnerException): {ex.InnerException?.Message}";

                MessageBox.Show(errorMessage, "Помилка (ToggleFavorite)");
            }
        }

        // --- Логіка Пагінації ---
        private bool CanGoToNextPage() => _isSearchPerformed && _currentPage < _totalPages;
        private async Task GoToNextPageAsync()
        {
            if (CanGoToNextPage())
            {
                CurrentPage++;
                await LoadBooksAsync();
            }
        }
        private bool CanGoToPreviousPage() => _isSearchPerformed && _currentPage > 1;
        private async Task GoToPreviousPageAsync()
        {
            if (CanGoToPreviousPage())
            {
                CurrentPage--;
                await LoadBooksAsync();
            }
        }
    }
}