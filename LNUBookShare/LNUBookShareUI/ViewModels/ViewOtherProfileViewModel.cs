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
    public class ViewOtherProfileViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly int _currentUserId;
        private readonly INavigationService _navigationService;

        private ProfileDto _profile;
        private ObservableCollection<OwnedBookDto> _allOwnedBooks = new ();
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;

        public ViewOtherProfileViewModel(IMediator mediator, INavigationService navigationService, int userId)
        {
            _mediator = mediator;
            _currentUserId = userId;
            _navigationService = navigationService;

            OwnedBooksView = CollectionViewSource.GetDefaultView(_allOwnedBooks);
            OwnedBooksView.Filter = FilterBooks;

            SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" },
            };

            LoadDataCommand = new RelayCommand(async () => await LoadProfileAsync());
            DeleteBookCommand = new RelayCommand<int>(async (bookId) => await DeleteBookAsync(bookId));

            SetFilterAllCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.All);
            SetFilterAvailableCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.Available);
            SetFilterIssuedCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.Issued);

            GoBackCommand = new RelayCommand<object>(GoBack);

            OpenBookDetailsCommand = new RelayCommand<int>(OpenBookDetails);

            _ = LoadProfileAsync();
        }

        public ProfileDto Profile
        {
            get => _profile;
            set => SetProperty(ref _profile, value);
        }

        public bool IsMyProfile => false;

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

        public ICommand OpenBookDetailsCommand { get; }

        private void OpenBookDetails(int bookId)
        {
            if (bookId > 0)
            {
                _navigationService.ShowBookDetails(bookId);
            }
        }

        private async Task LoadProfileAsync()
        {
            IsLoading = true;

            try
            {
                var query = new GetProfileQuery { UserId = _currentUserId };
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
                _ = MessageBox.Show($"Помилка завантаження профілю: {ex.Message}\n\n{ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteBookAsync(int bookId)
        {
            var command = new DeleteBookCommand
            {
                BookId = bookId,
                CurrentUserId = _currentUserId,
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