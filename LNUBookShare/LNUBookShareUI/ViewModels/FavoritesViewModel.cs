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
            get => _favoriteBooks;
            set => SetProperty(ref _favoriteBooks, value);
        }

        private int _totalResults;
        public int TotalResults
        {
            get => _totalResults;
            set => SetProperty(ref _totalResults, value);
        }
        private int _currentPage = 1;
        private int _totalPages = 1;
        private readonly int _pageSize = 10;
        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        // --- Сортування ---
        public Dictionary<BookSortCriteria, string> SortOptions { get; }
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        public BookSortCriteria SelectedSort
        {
            get => _selectedSort;
            set
            {
                if (SetProperty(ref _selectedSort, value))
                {
                    _ = LoadFavoritesAsync(); // Одразу перезавантажуємо
                }
            }
        }

        // --- Фільтрація ---
        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;
        public BookFilterStatus SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value))
                {
                    _ = LoadFavoritesAsync(); // Одразу перезавантажуємо
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



        // --- Конструктор ---
        public FavoritesViewModel(IMediator mediator, INavigationService navigationService)
        {
            _mediator = mediator;
            _navigationService = navigationService;

            // Ініціалізація словника для ComboBox сортування
            SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" }
            };
     

            // Команди
            GoBackCommand = new RelayCommand<object>(GoBack);
            RemoveFromFavoritesCommand = new RelayCommand<int>(async (bookId) => await RemoveFavoriteAsync(bookId));
            ClearFavoritesCommand = new RelayCommand(async () => await ClearFavoritesAsync());

            // Команди Фільтрів
            SetFilterAllCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.All);
            SetFilterAvailableCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.Available);
            SetFilterIssuedCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.Issued);

            // Пагінація
            NextPageCommand = new RelayCommand(async () => await GoToNextPageAsync(), CanGoToNextPage);
            PreviousPageCommand = new RelayCommand(async () => await GoToPreviousPageAsync(), CanGoToPreviousPage);

            // Завантажуємо дані при відкритті вікна
            _ = LoadFavoritesAsync();
        }

        // --- Методи ---

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

                var result = await _mediator.Send(query);

                // Розраховуємо сторінки
                _totalPages = (int)Math.Ceiling((double)result.TotalCount / _pageSize);
                if (_totalPages == 0) _totalPages = 1;

                App.Current.Dispatcher.Invoke(() =>
                {
                    FavoriteBooks.Clear();

                    foreach (var book in result.Items)
                    {
                        FavoriteBooks.Add(book);
                    }

                    TotalResults = result.TotalCount; // 👈 1. ЦЕЙ РЯДОК БУВ ВІДСУТНІЙ
                    CommandManager.InvalidateRequerySuggested(); // 👈 2. І ЦЕЙ (для кнопок ← →)
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження вподобаних: {ex.Message}", "Помилка");
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
                await _mediator.Send(command);

                // Оновлюємо список
                await LoadFavoritesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення з вподобаних: {ex.Message}", "Помилка");
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
                await _mediator.Send(command);
                await LoadFavoritesAsync(); // Оновлюємо список (тепер він буде порожній)
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка очищення: {ex.Message}", "Помилка");
            }
        }

        private bool CanGoToNextPage() => _currentPage < _totalPages;
        private async Task GoToNextPageAsync()
        {
            if (CanGoToNextPage())
            {
                CurrentPage++;
                await LoadFavoritesAsync();
            }
        }
        private bool CanGoToPreviousPage() => _currentPage > 1;
        private async Task GoToPreviousPageAsync()
        {
            if (CanGoToPreviousPage())
            {
                CurrentPage--;
                await LoadFavoritesAsync();
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