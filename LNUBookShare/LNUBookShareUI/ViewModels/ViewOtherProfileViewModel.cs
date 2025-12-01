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

//видалено ICommand DeleteBookCommand ,не потрібна для перегляду профілю іншого користувача,
//зайвий код
//Фільтри встановлюються через SetStatusFilterCommand
//Перейменовано метод GoBack() на CloseWindow
//покращено методи ApplySort() та FilterBooks() через switch, тому легше додати нові критерії
//назви змістовні 


namespace LNUBookShareUI.ViewModels
{
    public class ViewOtherProfileViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly int _currentUserId ;
        private readonly INavigationService _navigationService;

   
        private ProfileDto _profile;
        public ProfileDto Profile
        {
            get => this._profile;
            set => this.SetProperty(ref this._profile, value);
        }
     
        public bool IsMyProfile => false;

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
        public ICommand SetStatusFilterCommand { get; }

        public ICommand CloseWindowCommand { get; }

        public ICommand OpenBookDetailsCommand { get; }

        public ViewOtherProfileViewModel(IMediator mediator, INavigationService navigationService, int  userId)
        {
            this._mediator = mediator;
            this._currentUserId = userId;
            this._navigationService = navigationService;

            this.OwnedBooksView = CollectionViewSource.GetDefaultView(this._allOwnedBooks);
            this.OwnedBooksView.Filter = this.FilterBooks;

            this.SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" }
            };

            this.LoadDataCommand = new RelayCommand(async () => await this.LoadProfileAsync());
            

            
            SetStatusFilterCommand = new RelayCommand<BookFilterStatus>(
                status => SelectedStatusFilter = status);

            this.CloseWindowCommand = new RelayCommand<object>(this.CloseWindow);

            this.OpenBookDetailsCommand = new RelayCommand<int>(this.OpenBookDetails);

            _ = this.LoadProfileAsync();
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
                var query = new GetProfileQuery { UserId = _currentUserId };
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
                _ = MessageBox.Show($"Помилка завантаження профілю: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
             
          

        private void ApplyFilter()
        {
          
            this.OwnedBooksView.Refresh();
        }

        private bool FilterBooks(object item)
        {
            if (item is not OwnedBookDto book) return false;

            return SelectedStatusFilter switch
            {
                BookFilterStatus.All => true,
                BookFilterStatus.Available => book.Status == "available",
                BookFilterStatus.Issued => book.Status == "issued",
                _ => true
            };
        }

        private void CloseWindow(object window)
        {
            
            if (window is Window w)
            {
                w.Close();
            }
        }

        private void ApplySort()
        {
            
            this.OwnedBooksView.SortDescriptions.Clear();

            var direction = ListSortDirection.Ascending;
            string propertyName = SelectedSort switch
            {
                BookSortCriteria.Title => nameof(OwnedBookDto.Title),
                BookSortCriteria.Author => nameof(OwnedBookDto.Author),
                BookSortCriteria.Year => nameof(OwnedBookDto.Year),
                _ => nameof(OwnedBookDto.Title)
            };

            OwnedBooksView.SortDescriptions.Add(new SortDescription(propertyName, direction));
        }
    }
}
