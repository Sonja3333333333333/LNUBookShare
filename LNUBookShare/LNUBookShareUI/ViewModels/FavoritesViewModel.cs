//DRY / SRP  
//    Централізація логіки UI: Створено приватні методи ShowErrorMessage() та CloseWindow(object window).
//    Усунуто дублювання коду обробки помилок (MessageBox.Show) та логіки закриття вікна в різних методах

//SRP
//    Розділення логіки завантаження: Створено CalculateTotalPages(int totalCount) та UpdateUIState(result).	
//    Метод LoadFavoritesAsync тепер лише координує виклик IMediator та оновлення UI, дотримуючись SRP.

//SRP / DRY	
//    В LoadFavoritesAsync логіка оновлення колекції винесена в UpdateUIState і викликається через App.Current.Dispatcher.Invoke.	
//    Гарантує потокобезпечне оновлення UI та зменшує візуальний шум у основному методі завантаження.

//Meaningful Names	
//    Перейменував GoBack(object parameter) на CloseWindow(object window).	
//    Робить намір команди GoBackCommand більш очевидним.

//Meaningful Names	
//    Створено метод ResetPaginationAndLoad() для використання у властивостях SelectedSort та SelectedStatusFilter.	
//    Спрощує логіку при зміні фільтрів/сортування (встановлюємо CurrentPage = 1 та завантажуємо дані).



using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;
using LNUBookShareBLL.Features.Favorites;
using LNUBookShareUI.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using FavoriteBooksPaginatedResult = LNUBookShareBLL.DTOs.PaginatedResultDto<LNUBookShareBLL.DTOs.FavoriteBookCardDto>;

namespace LNUBookShareUI.ViewModels
{
    public class FavoritesViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;

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

        public Dictionary<BookSortCriteria, string> SortOptions { get; }
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        public BookSortCriteria SelectedSort
        {
            get => this._selectedSort;
            set
            {
                if (this.SetProperty(ref this._selectedSort, value))
                {
                    this.ResetPaginationAndLoad();
                }
            }
        }

        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;
        public BookFilterStatus SelectedStatusFilter
        {
            get => this._selectedStatusFilter;
            set
            {
                if (this.SetProperty(ref this._selectedStatusFilter, value))
                {
                    this.ResetPaginationAndLoad();
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

        public FavoritesViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession)
        {
            this._mediator = mediator;
            this._userSession = userSession;
            this._navigationService = navigationService;

            this.SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" }
            };

            this.GoBackCommand = new RelayCommand<object>(this.CloseWindow);
            this.RemoveFromFavoritesCommand = new RelayCommand<int>(async (bookId) => await this.RemoveFavoriteAsync(bookId));
            this.ClearFavoritesCommand = new RelayCommand(async () => await this.ClearFavoritesWithConfirmationAsync());

            // DRY: Команди фільтрації залишено лаконічними
            this.SetFilterAllCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.All);
            this.SetFilterAvailableCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.Available);
            this.SetFilterIssuedCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.Issued);

            this.NextPageCommand = new RelayCommand(async () => await this.GoToNextPageAsync(), this.CanGoToNextPage);
            this.PreviousPageCommand = new RelayCommand(async () => await this.GoToPreviousPageAsync(), this.CanGoToPreviousPage);

            this.OpenBookDetailsCommand = new RelayCommand<int>(this.OpenBookDetails);
            this.ViewOwnerProfileCommand = new RelayCommand<int>(this.ViewOwnerProfile);

            _ = this.LoadFavoritesAsync();
        }

        private void ShowErrorMessage(string message, string title = "Помилка")
        {
            _ = MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CloseWindow(object window)
        {
            if (window is Window w)
            {
                w.Close();
            }
        }
        private void ResetPaginationAndLoad()
        {
            this.CurrentPage = 1;
            _ = this.LoadFavoritesAsync();
        }

        private void CalculateTotalPages(int totalCount)
        {
            this._totalPages = (int)Math.Ceiling((double)totalCount / this._pageSize);
            if (this._totalPages == 0)
            {
                this._totalPages = 1;
            }
        }

        private void UpdateUIState(FavoriteBooksPaginatedResult result)
        {
            this.FavoriteBooks = new ObservableCollection<FavoriteBookCardDto>(result.Items);

            this.TotalResults = result.TotalCount;
            CommandManager.InvalidateRequerySuggested();
        }


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

        public async Task LoadFavoritesAsync()
        {
            try
            {
                var query = new GetFavoriteBooksQuery
                {
                    CurrentUserId = this._userSession.GetUserId(),
                    FilterBy = this.SelectedStatusFilter,
                    SortBy = this.SelectedSort,
                    PageNumber = this.CurrentPage,
                    PageSize = this._pageSize
                };

                FavoriteBooksPaginatedResult result = await this._mediator.Send(query);

                this.CalculateTotalPages(result.TotalCount);

                App.Current.Dispatcher.Invoke(() => this.UpdateUIState(result));
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage($"Помилка завантаження вподобаних: {ex.Message}"); 
            }
        }

        private async Task RemoveFavoriteAsync(int bookId)
        {
            try
            {
                var command = new ToggleFavoriteCommand
                {
                    BookId = bookId,
                    UserId = this._userSession.GetUserId()
                };

                _ = await this._mediator.Send(command);
                await this.LoadFavoritesAsync();
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage($"Помилка видалення з вподобаних: {ex.Message}"); 
            }
        }

        private async Task ClearFavoritesWithConfirmationAsync()
        {
            var result = MessageBox.Show("Ви впевнені, що хочете видалити ВСІ книги з вподобань?",
                                             "Підтвердження",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var command = new ClearFavoritesCommand { UserId = this._userSession.GetUserId() };
                _ = await this._mediator.Send(command);
                await this.LoadFavoritesAsync();
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage($"Помилка очищення: {ex.Message}"); 
            }
        }


        private bool CanGoToNextPage() => this.CurrentPage < this._totalPages;
        private async Task GoToNextPageAsync()
        {
            if (this.CanGoToNextPage())
            {
                this.CurrentPage++;
                await this.LoadFavoritesAsync();
            }
        }
        private bool CanGoToPreviousPage() => this.CurrentPage > 1;
        private async Task GoToPreviousPageAsync()
        {
            if (this.CanGoToPreviousPage())
            {
                this.CurrentPage--;
                await this.LoadFavoritesAsync();
            }
        }
    }
}