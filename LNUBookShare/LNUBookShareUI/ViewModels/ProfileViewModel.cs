using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Profile;

using LNUBookShareUI.Common;

using MediatR;

namespace LNUBookShareUI.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSession _userSession;
        private readonly INavigationService _navigationService;

        private ProfileDto _profile;
        private bool _isMyProfile;
        private ObservableCollection<OwnedBookDto> _allOwnedBooks = new();
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;

        public ProfileViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession)
        {
            _mediator = mediator;
            IsMyProfile = true;
            _navigationService = navigationService;
            _userSession = userSession;

            OwnedBooksView = CollectionViewSource.GetDefaultView(_allOwnedBooks);
            OwnedBooksView.Filter = FilterBooks;

            SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" }
            };

            LoadDataCommand = new RelayCommand(async () => await LoadProfileAsync());
            DeleteBookCommand = new RelayCommand<int>(async (bookId) => await DeleteBookAsync(bookId));

            SetFilterAllCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.All);
            SetFilterAvailableCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.Available);
            SetFilterIssuedCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.Issued);

            GoBackCommand = new RelayCommand<object>(GoBack);

            OpenBookDetailsCommand = new RelayCommand<int>(OpenBookDetails);
            OpenEditProfileCommand = new RelayCommand(async () => await OpenEditProfile());
            OpenAddBookCommand = new RelayCommand(async () => await OpenAddBook());
            OpenEditBookCommand = new RelayCommand<int>(async (id) => await OpenEditBook(id));

            _ = LoadProfileAsync();
        }

        public ProfileDto Profile
        {
            get => _profile;
            set => SetProperty(ref _profile, value);
        }

        public bool IsMyProfile
        {
            get => _isMyProfile;
            set
            {
                _isMyProfile = value;
                OnPropertyChanged(nameof(IsMyProfile));
            }
        }

        public ICollectionView OwnedBooksView { get; }

        public Dictionary<BookSortCriteria, string> SortOptions { get; }

        public BookSortCriteria SelectedSort
        {
            get => _selectedSort;
            set
            {
                if (SetProperty(ref _selectedSort, value))
                {
                    ApplySort();
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
                    ApplyFilter();
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

        private async Task OpenEditBook(int bookId)
        {
            if (bookId == 0)
            {
                return;
            }

            try
            {
                await _navigationService.ShowEditBookAsync(bookId);
                await LoadProfileAsync();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Не вдалося відкрити редактор: {ex.Message}", "Помилка");
            }
        }

        private async Task OpenAddBook()
        {
            await _navigationService.ShowAddBookAsync();
            await LoadProfileAsync();
        }

        private async Task OpenEditProfile()
        {
            try
            {
                await _navigationService.ShowEditProfile();
                await LoadProfileAsync();
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
                _navigationService.ShowBookDetails(bookId);
            }
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                int userId = _userSession.GetUserId();

                var query = new GetProfileQuery { UserId = userId };
                var result = await _mediator.Send(query);

                Profile = result;

                App.Current.Dispatcher.Invoke(() =>
                {
                    _allOwnedBooks.Clear();
                    foreach (var book in result.OwnedBooks)
                    {
                        _allOwnedBooks.Add(book);
                    }
                });

                ApplySort();
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
                _ = await _mediator.Send(command);

                var bookToRemove = _allOwnedBooks.FirstOrDefault(b => b.BookId == bookId);
                if (bookToRemove != null)
                {
                    _ = _allOwnedBooks.Remove(bookToRemove);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Помилка видалення: {ex.Message}");
            }
        }

        private void ApplyFilter()
        {
            OwnedBooksView.Refresh();
        }

        private bool FilterBooks(object item)
        {
            if (SelectedStatusFilter == BookFilterStatus.All)
            {
                return true;
            }

            var book = (OwnedBookDto)item;

            if (SelectedStatusFilter == BookFilterStatus.Available)
            {
                return book.Status == "available";
            }

            if (SelectedStatusFilter == BookFilterStatus.Issued)
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
            OwnedBooksView.SortDescriptions.Clear();

            switch (SelectedSort)
            {
                case BookSortCriteria.Title:
                    OwnedBooksView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
                    break;
                case BookSortCriteria.Author:
                    OwnedBooksView.SortDescriptions.Add(new SortDescription("Author", ListSortDirection.Ascending));
                    break;
                case BookSortCriteria.Year:
                    OwnedBooksView.SortDescriptions.Add(new SortDescription("Year", ListSortDirection.Ascending));
                    break;
            }
        }
    }
}