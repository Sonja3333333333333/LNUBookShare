using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;
using LNUBookShareBLL.Features.Favorites;
using LNUBookShareUI.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LNUBookShareUI.ViewModels
{
    public class FavoritesViewModel : ViewModelBase
    {
        // --- Поля ---
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;

        // "Захардкодили" ID користувача для тестування
        private readonly int _currentUserId = 1;

        // --- Властивості ---
        private ObservableCollection<FavoriteBookCardDto> _favoriteBooks = new();
        public ObservableCollection<FavoriteBookCardDto> FavoriteBooks
        {
            get => this._favoriteBooks;
            set => this.SetProperty(ref this._favoriteBooks, value);
        }

        private int _totalResults;
        public int TotalResults
        {
            get => this._totalResults;
            set => this.SetProperty(ref this._totalResults, value);
        }
        private int _currentPage = 1;
        private int _totalPages = 1;
        private readonly int _pageSize = 10;
        public int CurrentPage
        {
            get => this._currentPage;
            set => this.SetProperty(ref this._currentPage, value);
        }

        // --- Сортування ---
        public Dictionary<BookSortCriteria, string> SortOptions { get; }
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        public BookSortCriteria SelectedSort
        {
            get => this._selectedSort;
            set
            {
                if (this.SetProperty(ref this._selectedSort, value))
                {
                    _ = this.LoadFavoritesAsync(); // Одразу перезавантажуємо
                }
            }
        }

        // --- Фільтрація ---
        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;
        public BookFilterStatus SelectedStatusFilter
        {
            get => this._selectedStatusFilter;
            set
            {
                if (this.SetProperty(ref this._selectedStatusFilter, value))
                {
                    _ = this.LoadFavoritesAsync(); // Одразу перезавантажуємо
                }
            }
        }

        // --- Команди ---
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



        // --- Конструктор ---
        public FavoritesViewModel(IMediator mediator, INavigationService navigationService)
        {
            this._mediator = mediator;
            this._navigationService = navigationService;

            // Ініціалізація словника для ComboBox сортування
            this.SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" }
            };


            // Команди
            this.GoBackCommand = new RelayCommand<object>(this.GoBack);
            this.RemoveFromFavoritesCommand = new RelayCommand<int>(async (bookId) => await this.RemoveFavoriteAsync(bookId));
            this.ClearFavoritesCommand = new RelayCommand(async () => await this.ClearFavoritesAsync());

            // Команди Фільтрів
            this.SetFilterAllCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.All);
            this.SetFilterAvailableCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.Available);
            this.SetFilterIssuedCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.Issued);

            // Пагінація
            this.NextPageCommand = new RelayCommand(async () => await this.GoToNextPageAsync(), this.CanGoToNextPage);
            this.PreviousPageCommand = new RelayCommand(async () => await this.GoToPreviousPageAsync(), this.CanGoToPreviousPage);

            this.OpenBookDetailsCommand = new RelayCommand<int>(this.OpenBookDetails);
            this.ViewOwnerProfileCommand = new RelayCommand<int>(this.ViewOwnerProfile);

            // Завантажуємо дані при відкритті вікна
            _ = this.LoadFavoritesAsync();
        }

        // --- Методи ---
        private void OpenBookDetails(int bookId)
        {
            if (bookId > 0)
            {
                this._navigationService.ShowBookDetails(bookId);
            }
        }

        private void ViewOwnerProfile(int ownerId)
        {
            if (ownerId > 0)
            {
                this._navigationService.ShowViewProfile(ownerId);
            }
        }

        private async Task LoadFavoritesAsync()
        {
            try
            {
                var query = new GetFavoriteBooksQuery
                {
                    CurrentUserId = _currentUserId,
                    FilterBy = this.SelectedStatusFilter,
                    SortBy = this.SelectedSort,
                    PageNumber = this._currentPage, // 👈 ДОДАНО ПАГІНАЦІЮ
                    PageSize = this._pageSize
                };

                var result = await this._mediator.Send(query);

                // Розраховуємо сторінки
                this._totalPages = (int)Math.Ceiling((double)result.TotalCount / this._pageSize);
                if (this._totalPages == 0) this._totalPages = 1;

                App.Current.Dispatcher.Invoke(() =>
                {
                    this.FavoriteBooks.Clear();

                    foreach (var book in result.Items)
                    {
                        this.FavoriteBooks.Add(book);
                    }

                    this.TotalResults = result.TotalCount; // 👈 1. ЦЕЙ РЯДОК БУВ ВІДСУТНІЙ
                    CommandManager.InvalidateRequerySuggested(); // 👈 2. І ЦЕЙ (для кнопок ← →)
                });
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Помилка завантаження вподобаних: {ex.Message}", "Помилка");
            }
        }

        // Кнопка "Видалити" (сердечко)
        private async Task RemoveFavoriteAsync(int bookId)
        {
            try
            {
                var command = new ToggleFavoriteCommand
                {
                    BookId = bookId,
                    UserId = _currentUserId
                };

                // Викликаємо BLL (він видалить книгу з вподобаних)
                _ = await this._mediator.Send(command);

                // Оновлюємо список
                await this.LoadFavoritesAsync();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Помилка видалення з вподобаних: {ex.Message}", "Помилка");
            }
        }

        // Кнопка "Очистити вподобання"
        private async Task ClearFavoritesAsync()
        {
            // Запитуємо користувача
            var result = MessageBox.Show("Ви впевнені, що хочете видалити ВСІ книги з вподобань?",
                                         "Підтвердження",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var command = new ClearFavoritesCommand { UserId = _currentUserId };
                _ = await this._mediator.Send(command);
                await this.LoadFavoritesAsync(); // Оновлюємо список (тепер він буде порожній)
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Помилка очищення: {ex.Message}", "Помилка");
            }
        }

        private bool CanGoToNextPage() => this._currentPage < this._totalPages;
        private async Task GoToNextPageAsync()
        {
            if (this.CanGoToNextPage())
            {
                this.CurrentPage++;
                await this.LoadFavoritesAsync();
            }
        }
        private bool CanGoToPreviousPage() => this._currentPage > 1;
        private async Task GoToPreviousPageAsync()
        {
            if (this.CanGoToPreviousPage())
            {
                this.CurrentPage--;
                await this.LoadFavoritesAsync();
            }
        }

        // Кнопка "Назад"
        private void GoBack(object parameter)
        {
            if (parameter is Window w)
            {
                w.Close();
            }
        }
    }
}