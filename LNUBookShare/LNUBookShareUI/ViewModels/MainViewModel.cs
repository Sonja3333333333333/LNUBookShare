using MediatR;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Favorites;
using LNUBookShareUI.Common;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using System.Windows;

//Прибрано дублювання:
//Перехід сторінок об’єднано в метод ChangePageAsync(int delta).
//Умови переходів об’єднані в CanChangePage().
//Фільтри встановлюються через SetFilterCommand замість трьох команд.
//уникнено дублювання і перенесено логіку в метод RestartSearch()
//Meaningful Names: назви методів змістовні, тому тут все добре


namespace LNUBookShareUI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;
        private bool _isSearchPerformed = false; 

        private int _currentPage = 1;
        private int _totalPages = 1;
        private readonly int _pageSize = 10;

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




        public Dictionary<BookSortCriteria, string> SortOptions { get; }
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        public BookSortCriteria SelectedSort
        {
            get => this._selectedSort;
            set
            {                
                if (this.SetProperty(ref this._selectedSort, value) && this._isSearchPerformed)
                {
                    RestartSearch();
                }
            }
        }

        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;
        public BookFilterStatus SelectedStatusFilter
        {
            get => this._selectedStatusFilter;
            set
            {
                if (this.SetProperty(ref this._selectedStatusFilter, value) && this._isSearchPerformed)
                {
                    RestartSearch();
                }
            }
        }

        public ICommand LoadBooksCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        //public ICommand SetFilterAllCommand { get; }
        //public ICommand SetFilterAvailableCommand { get; }
        //public ICommand SetFilterIssuedCommand { get; }
        public ICommand SetFilterCommand { get; }//1 filter command
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand OpenProfileCommand { get; }
        public ICommand ViewOwnerProfileCommand { get; }
        public ICommand OpenFavoritesCommand { get; }

        public ICommand OpenBookDetailsCommand { get; }

        public MainViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession)
        {
            this._mediator = mediator;
            this._navigationService = navigationService;
            this._userSession = userSession;

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


            this.LoadBooksCommand = new RelayCommand(async () => await this.SearchAsync());
            this.ToggleFavoriteCommand = new RelayCommand<int>(async (id) => await this.ToggleFavoriteAsync(id));
            
            this.SetFilterCommand = new RelayCommand<BookFilterStatus>(filter =>
            {
                SelectedStatusFilter = filter;
            });            

            NextPageCommand = new RelayCommand(async () => await ChangePageAsync(1), () => CanChangePage(1));
            PreviousPageCommand = new RelayCommand(async () => await ChangePageAsync(-1), () => CanChangePage(-1));

            this.OpenProfileCommand = new RelayCommand(this.OpenProfile);
            this.OpenFavoritesCommand = new RelayCommand(this.OpenFavorites);

            this.ViewOwnerProfileCommand = new RelayCommand<int>(this.ViewOwnerProfile);

            this.OpenBookDetailsCommand = new RelayCommand<int>(this.OpenBookDetails);
        }

        private void OpenProfile()
        {
            this._navigationService.ShowProfile();
        }
        private void ViewOwnerProfile(int ownerId)
        {

            this._navigationService.ShowViewProfile(ownerId);
        }
        private void OpenFavorites()
        {
            this._navigationService.ShowFavorites();
        }        

        private async Task SearchAsync()
        {
            this.CurrentPage = 1; 
            await this.LoadBooksAsync();
        }

        private void OpenBookDetails(int bookId)
        {
            if (bookId > 0)
            {
                this._navigationService.ShowBookDetails(bookId);
            }
        }
        private void RestartSearch()
        {
            _currentPage = 1;
            _ = LoadBooksAsync();
        }
        private async Task LoadBooksAsync()
        {
            try
            {
                var query = new GetBooksQuery
                {
                    CurrentUserId = _userSession.GetUserId(),
                    SearchTerm = this.SearchTerm,
                    SearchBy = this.SelectedSearchCriteria,
                    PageNumber = this._currentPage,
                    PageSize = this._pageSize,
                    FilterBy = this.SelectedStatusFilter,
                    SortBy = this.SelectedSort
                };

                var result = await this._mediator.Send(query);

                this._isSearchPerformed = true; 

                this._totalPages = (int)Math.Ceiling((double)result.TotalCount / this._pageSize);
                if (this._totalPages == 0)
                {
                    this._totalPages = 1;
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    this.Books.Clear();
                    foreach (var book in result.Items)
                    {
                        this.Books.Add(book);
                    }
                    this.TotalResults = result.TotalCount;
                    CommandManager.InvalidateRequerySuggested(); 
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
                    UserId = _userSession.GetUserId()
                };

                _ = await this._mediator.Send(command);
                await this.LoadBooksAsync();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Помилка: {ex.Message}\n\n" +
                                      $"Деталі (InnerException): {ex.InnerException?.Message}";

                _ = MessageBox.Show(errorMessage, "Помилка (ToggleFavorite)");
            }
        }

        private bool CanChangePage(int delta) =>
            _isSearchPerformed &&
            _currentPage + delta > 1 &&
            _currentPage + delta < _totalPages;

        private async Task ChangePageAsync(int delta)
        {
            if (!CanChangePage(delta)) return;

            _currentPage += delta;
            await LoadBooksAsync();
        }



        /*
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
        }*/
    }
}