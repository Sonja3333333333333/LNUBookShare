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
using LNUBookShareUI.Views;

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
            get => this._books;
            set => this.SetProperty(ref this._books, value);
        }

        private int _totalResults;
        public int TotalResults
        {
            get => this._totalResults;
            set => this.SetProperty(ref this._totalResults, value);
        }

        public int CurrentPage
        {
            get => this._currentPage;
            set => this.SetProperty(ref this._currentPage, value);
        }

        // --- Критерії Пошуку ---
        public Dictionary<BookSearchCriteria, string> SearchOptions { get; }
        private BookSearchCriteria _selectedSearchCriteria = BookSearchCriteria.Title;
        public BookSearchCriteria SelectedSearchCriteria
        {
            get => this._selectedSearchCriteria;
            set => this.SetProperty(ref this._selectedSearchCriteria, value);
        }

        private string _searchTerm = "";
        public string SearchTerm
        {
            get => this._searchTerm;
            set => this.SetProperty(ref this._searchTerm, value);
        }

        // --- Критерії Сортування ---
        public Dictionary<BookSortCriteria, string> SortOptions { get; }
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        public BookSortCriteria SelectedSort
        {
            get => this._selectedSort;
            set
            {
                bool valueChanged = this.SetProperty(ref this._selectedSort, value);
                if (valueChanged && this._isSearchPerformed)
                {
                    this.CurrentPage = 1;
                    _ = this.LoadBooksAsync();
                }
            }
        }

        // --- Критерії Фільтрації ---
        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;
        public BookFilterStatus SelectedStatusFilter
        {
            get => this._selectedStatusFilter;
            set
            {
                if (this.SetProperty(ref this._selectedStatusFilter, value) && this._isSearchPerformed)
                {
                    this.CurrentPage = 1;
                    _ = this.LoadBooksAsync();
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

        public ICommand OpenBookDetailsCommand { get; }


        // --- Конструктор ---
        public MainViewModel(IMediator mediator, INavigationService navigationService)
        {
            this._mediator = mediator;
            this._navigationService = navigationService;

            this.SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" },
                { BookSortCriteria.Category, "Категорія"}
            };

            this.SearchOptions = new Dictionary<BookSearchCriteria, string>
            {
                { BookSearchCriteria.Title, "Назва" },
                { BookSearchCriteria.Author, "Автор" },
                { BookSearchCriteria.ISBN, "ISBN" },
                { BookSearchCriteria.Category, "Категорія"}

            };



            // Зв'язуємо команди
            this.LoadBooksCommand = new RelayCommand(async () => await this.SearchAsync());
            this.ToggleFavoriteCommand = new RelayCommand<int>(async (id) => await this.ToggleFavoriteAsync(id));

            // Фільтри
            this.SetFilterAllCommand = new RelayCommand(() => this.SetFilter(BookFilterStatus.All));
            this.SetFilterAvailableCommand = new RelayCommand(() => this.SetFilter(BookFilterStatus.Available));
            this.SetFilterIssuedCommand = new RelayCommand(() => this.SetFilter(BookFilterStatus.Issued));

            // Пагінація
            this.NextPageCommand = new RelayCommand(async () => await this.GoToNextPageAsync(), this.CanGoToNextPage);
            this.PreviousPageCommand = new RelayCommand(async () => await this.GoToPreviousPageAsync(), this.CanGoToPreviousPage);

            this.OpenProfileCommand = new RelayCommand(this.OpenProfile);
            this.OpenFavoritesCommand = new RelayCommand(this.OpenFavorites);

            this.ViewOwnerProfileCommand = new RelayCommand<int>(this.ViewOwnerProfile);

            this.OpenBookDetailsCommand = new RelayCommand<int>(this.OpenBookDetails);
        }

        // --- Логіка ---
        private void OpenProfile()
        {
            // Просто викликаємо метод із сервісу
            this._navigationService.ShowProfile();
        }
        private void ViewOwnerProfile(int ownerId)
        {

            this._navigationService.ShowViewProfile(ownerId);
        }
        private void OpenFavorites()
        {
            // Робимо те саме, що й для профілю
            this._navigationService.ShowFavorites();
        }

        private void SetFilter(BookFilterStatus status)
        {
            this.SelectedStatusFilter = status;
        }

        private async Task SearchAsync()
        {
            this.CurrentPage = 1; // Скидаємо сторінку при кожному новому пошуку
            await this.LoadBooksAsync();
        }

        private void OpenBookDetails(int bookId)
        {
            if (bookId > 0)
            {
                this._navigationService.ShowBookDetails(bookId);
            }
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

                var result = await this._mediator.Send(query);

                this._isSearchPerformed = true; // "Піднімаємо прапорець"

                this._totalPages = (int)Math.Ceiling((double)result.TotalCount / this._pageSize);
                if (this._totalPages == 0) this._totalPages = 1;

                App.Current.Dispatcher.Invoke(() =>
                {
                    this.Books.Clear();
                    foreach (var book in result.Items)
                    {
                        this.Books.Add(book);
                    }
                    this.TotalResults = result.TotalCount;
                    CommandManager.InvalidateRequerySuggested(); // Оновлюємо стан кнопок ← →
                });
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Сталася помилка: {ex.Message}", "Помилка завантаження");
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

                _ = await this._mediator.Send(command);
                await this.LoadBooksAsync();
            }
            catch (Exception ex)
            {
                // Формуємо деталізоване повідомлення
                string errorMessage = $"Помилка: {ex.Message}\n\n" +
                                      $"Деталі (InnerException): {ex.InnerException?.Message}";

                _ = MessageBox.Show(errorMessage, "Помилка (ToggleFavorite)");
            }
        }

        // --- Логіка Пагінації ---
        private bool CanGoToNextPage() => this._isSearchPerformed && this._currentPage < this._totalPages;
        private async Task GoToNextPageAsync()
        {
            if (this.CanGoToNextPage())
            {
                this.CurrentPage++;
                await this.LoadBooksAsync();
            }
        }
        private bool CanGoToPreviousPage() => this._isSearchPerformed && this._currentPage > 1;
        private async Task GoToPreviousPageAsync()
        {
            if (this.CanGoToPreviousPage())
            {
                this.CurrentPage--;
                await this.LoadBooksAsync();
            }
        }
    }
}