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

namespace LNUBookShareUI.ViewModels
{
    public class FavoritesViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;
        private readonly int _pageSize = 10;

        private ObservableCollection<FavoriteBookCardDto> _favoriteBooks = new ();
        private int _totalResults;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;

        public FavoritesViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession)
        {
            _mediator = mediator;
            _userSession = userSession;
            _navigationService = navigationService;

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
                _navigationService.ShowBookDetails(bookId);
            }
        }

        private void ViewOwnerProfile(int ownerId)
        {
            if (ownerId > 0)
            {
                _navigationService.ShowViewProfile(ownerId);
            }
        }

        private async Task LoadFavoritesAsync()
        {
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
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Помилка завантаження вподобаних: {ex.Message}", "Помилка");
            }
        }

        private async Task RemoveFavoriteAsync(int bookId)
        {
            try
            {
                var command = new ToggleFavoriteCommand
                {
                    BookId = bookId,
                    UserId = _userSession.GetUserId(),
                };

                _ = await _mediator.Send(command);

                await LoadFavoritesAsync();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Помилка видалення з вподобаних: {ex.Message}", "Помилка");
            }
        }

        private async Task ClearFavoritesAsync()
        {
            var result = MessageBox.Show(
                "Ви впевнені, що хочете видалити ВСІ книги з вподобань?",
                "Підтвердження",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var command = new ClearFavoritesCommand { UserId = _userSession.GetUserId() };
                _ = await _mediator.Send(command);
                await LoadFavoritesAsync();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Помилка очищення: {ex.Message}", "Помилка");
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

        private void GoBack(object parameter)
        {
            if (parameter is Window w)
            {
                w.Close();
            }
        }
    }
}