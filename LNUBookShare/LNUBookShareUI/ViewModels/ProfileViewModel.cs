using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Profile;
using LNUBookShareUI.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;


namespace LNUBookShareUI.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSession _userSession;
        private readonly INavigationService _navigationService;

        private ProfileDto _profile;
        public ProfileDto Profile
        {
            get => this._profile;
            set => this.SetProperty(ref this._profile, value);
        }

        private bool _isMyProfile;
        public bool IsMyProfile
        {
            get => this._isMyProfile;
            set
            {
                this._isMyProfile = value;
                this.OnPropertyChanged(nameof(this.IsMyProfile)); 
            }
        }

   
        private ObservableCollection<OwnedBookDto> _allOwnedBooks = new();

        public ICollectionView OwnedBooksView { get; }

        public Dictionary<BookSortCriteria, string> SortOptions { get; }

        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        public BookSortCriteria SelectedSort
        {
            get => this._selectedSort;
            set
            {
                if (this.SetProperty(ref this._selectedSort, value))
                {
                    this.ApplySort(); 
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
                    this.ApplyFilter();
                }
            }
        }
        

        public ICommand LoadDataCommand { get; }
        public ICommand DeleteBookCommand { get; }
        public ICommand SetFilterAllCommand { get; }
        public ICommand SetFilterAvailableCommand { get; }
        public ICommand SetFilterIssuedCommand { get; }

        public ICommand GoBackCommand { get; }
        public ICommand OpenEditProfileCommand { get; }

        public ICommand OpenBookDetailsCommand { get; }

        public ICommand OpenAddBookCommand { get; }

        public ICommand OpenEditBookCommand { get; }

        public ProfileViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession)
        {
            this._mediator = mediator;
            this.IsMyProfile = true;
            this._navigationService = navigationService;
            this._userSession = userSession;

            this.OwnedBooksView = CollectionViewSource.GetDefaultView(this._allOwnedBooks);
            this.OwnedBooksView.Filter = this.FilterBooks; 

            
            this.SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" }
            };

     
            this.LoadDataCommand = new RelayCommand(async () => await this.LoadProfileAsync());
            this.DeleteBookCommand = new RelayCommand<int>(async (bookId) => await this.DeleteBookAsync(bookId));

            this.SetFilterAllCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.All);
            this.SetFilterAvailableCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.Available);
            this.SetFilterIssuedCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.Issued);

            this.GoBackCommand = new RelayCommand<object>(this.GoBack);

            this.OpenBookDetailsCommand = new RelayCommand<int>(this.OpenBookDetails);

            this.OpenEditProfileCommand = new RelayCommand(async () => await this.OpenEditProfile());

            this.OpenAddBookCommand = new RelayCommand(async () => await this.OpenAddBook());

            this.OpenEditBookCommand = new RelayCommand<int>(async (id) => await this.OpenEditBook(id));

            _ = this.LoadProfileAsync();
           
        }

        private async Task OpenEditBook(int bookId)
        {
            if (bookId == 0)
            {
                return;
            }

            try
            {
                await this._navigationService.ShowEditBookAsync(bookId);
               
                await this.LoadProfileAsync();
            }
            catch (Exception ex)
            {
                
                _ = MessageBox.Show($"Не вдалося відкрити редактор: {ex.Message}", "Помилка");
            }
        }


        private async Task OpenAddBook()
        {
            
            await this._navigationService.ShowAddBookAsync();

          
            await this.LoadProfileAsync();
        }

        private async Task OpenEditProfile()
        {
            try
            {
                
                await this._navigationService.ShowEditProfile();
                await this.LoadProfileAsync();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Не вдалося відкрити редактор: {ex.Message}");
            }
        }



        private void OpenBookDetails(int bookId)
        {
            if (bookId > 0)
            {
                this._navigationService.ShowBookDetails(bookId);
            }
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                int userId = _userSession.GetUserId();

                var query = new GetProfileQuery { UserId = userId };
                var result = await this._mediator.Send(query);

            
                this.Profile = result;

                App.Current.Dispatcher.Invoke(() =>
                {
                    this._allOwnedBooks.Clear();
                    foreach (var book in result.OwnedBooks)
                    {
                        this._allOwnedBooks.Add(book);
                    }
                });

           
                this.ApplySort();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Помилка завантаження профілю: {ex.Message}");
            }
        }

        private async Task DeleteBookAsync(int bookId)
        {
            
            var command = new DeleteBookCommand
            {
                BookId = bookId,
                CurrentUserId = _userSession.GetUserId()
            };

            try
            {
                _ = await this._mediator.Send(command);
              
                var bookToRemove = this._allOwnedBooks.FirstOrDefault(b => b.BookId == bookId);
                if (bookToRemove != null)
                {
                    _ = this._allOwnedBooks.Remove(bookToRemove);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Помилка видалення: {ex.Message}");
            }
        }

      
        private void ApplyFilter()
        {
            this.OwnedBooksView.Refresh();
        }

        private bool FilterBooks(object item)
        {
            if (this.SelectedStatusFilter == BookFilterStatus.All)
            {
                return true; 
            }

            var book = (OwnedBookDto)item;

            if (this.SelectedStatusFilter == BookFilterStatus.Available)
            {
                return book.Status == "available";
            }

            if (this.SelectedStatusFilter == BookFilterStatus.Issued)
            {
                return book.Status == "issued";
            }

            return true;
        }

        private void GoBack(object window)
        {
            
            if (window is Window w)
            {
                w.Close();
            }
        }

        private void ApplySort()
        {
           
            this.OwnedBooksView.SortDescriptions.Clear();
            
            switch (this.SelectedSort)
            {
                case BookSortCriteria.Title:
                    this.OwnedBooksView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
                    break;
                case BookSortCriteria.Author:
                    this.OwnedBooksView.SortDescriptions.Add(new SortDescription("Author", ListSortDirection.Ascending));
                    break;
                case BookSortCriteria.Year:
                    this.OwnedBooksView.SortDescriptions.Add(new SortDescription("Year", ListSortDirection.Ascending));
                    break;
            }
        }
    }
}
