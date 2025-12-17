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
using Microsoft.Extensions.Logging;

namespace LNUBookShareUI.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSession _userSession;
        private readonly INavigationService _navigationService;

        private readonly ILogger<ProfileViewModel> _logger;

        private ProfileDto _profile;
        private bool _isMyProfile;
        private ObservableCollection<OwnedBookDto> _allOwnedBooks = new();
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;

        public ProfileViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession, ILogger<ProfileViewModel> logger)
        {
            _mediator = mediator;
            IsMyProfile = true;
            _navigationService = navigationService;
            _userSession = userSession;
            _logger = logger;

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

            LogoutCommand = new RelayCommand<object>(OnLogout);

            OpenBookDetailsCommand = new RelayCommand<int>(OpenBookDetails);
            OpenEditProfileCommand = new RelayCommand(async () => await OpenEditProfile());
            OpenAddBookCommand = new RelayCommand(async () => await OpenAddBook());
            OpenEditBookCommand = new RelayCommand<int>(async (id) => await OpenEditBook(id));

            _logger.LogInformation("ProfileViewModel ініціалізовано для користувача ID: {UserId}.", _userSession.GetUserId());

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
                    _logger.LogInformation("Користувач ID: {UserId} змінив сортування своїх книг на: {Sort}.", _userSession.GetUserId(), value);
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
                    _logger.LogInformation("Користувач ID: {UserId} змінив фільтр своїх книг на: {Filter}.", _userSession.GetUserId(), value);
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

        public ICommand LogoutCommand { get; }

        public ICommand OpenEditProfileCommand { get; }

        public ICommand OpenBookDetailsCommand { get; }

        public ICommand OpenAddBookCommand { get; }

        public ICommand OpenEditBookCommand { get; }

        private async Task OpenEditBook(int bookId)
        {
            int userId = _userSession.GetUserId();
            _logger.LogInformation("Користувач ID: {UserId} відкриває редагування своєї книги ID: {BookId}.", userId, bookId);

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
                _logger.LogError(ex, "Помилка при спробі відкрити редагування книги ID: {BookId} для користувача ID: {UserId}.", bookId, userId);
                _ = MessageBox.Show($"Не вдалося відкрити редактор: {ex.Message}", "Помилка");
            }
        }

        private async Task OpenAddBook()
        {
            int userId = _userSession.GetUserId();
            _logger.LogInformation("Користувач ID: {UserId} натиснув 'Додати книгу'.", userId);

            await _navigationService.ShowAddBookAsync();
            await LoadProfileAsync();
        }

        private async Task OpenEditProfile()
        {
            int userId = _userSession.GetUserId();
            _logger.LogInformation("Користувач ID: {UserId} переходить до редагування профілю.", userId);

            try
            {
                await _navigationService.ShowEditProfile();
                await LoadProfileAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка переходу до редагування профілю користувача ID: {UserId}.", userId);
                _ = MessageBox.Show($"Не вдалося відкрити редактор: {ex.Message}");
            }
        }

        private void OpenBookDetails(int bookId)
        {
            if (bookId > 0)
            {
                _logger.LogInformation("Користувач ID: {UserId} переглядає деталі своєї книги ID: {BookId}.", _userSession.GetUserId(), bookId);
                _navigationService.ShowBookDetails(bookId);
            }
        }

        private async Task LoadProfileAsync()
        {
            IsLoading = true;
            int userId = _userSession.GetUserId();
            _logger.LogInformation("Завантаження даних особистого профілю для користувача ID: {UserId}.", userId);

            try
            {
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
                _logger.LogInformation("Профіль завантажено. Користувач ID: {UserId} має {Count} книг.", userId, result.OwnedBooks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка завантаження профілю для користувача ID: {UserId}.", userId);
                _ = MessageBox.Show($"Помилка завантаження профілю: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteBookAsync(int bookId)
        {
            IsLoading = true;

            int userId = _userSession.GetUserId();
            _logger.LogInformation("Користувач ID: {UserId} ініціював видалення своєї книги ID: {BookId}.", userId, bookId);

            try
            {
                var command = new DeleteBookCommand
                {
                    BookId = bookId,
                    CurrentUserId = _userSession.GetUserId(),
                };
                _ = await _mediator.Send(command);

                var bookToRemove = _allOwnedBooks.FirstOrDefault(b => b.BookId == bookId);
                if (bookToRemove != null)
                {
                    _ = _allOwnedBooks.Remove(bookToRemove);
                }

                _logger.LogInformation("Книгу ID: {BookId} успішно видалено з профілю користувача ID: {UserId}.", bookId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка видалення книги ID: {BookId} користувачем ID: {UserId}.", bookId, userId);
                _ = MessageBox.Show($"Помилка видалення: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
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
            _logger.LogInformation("Користувач ID: {UserId} виходить з профілю.", _userSession.GetUserId());
            if (window is Window w)
            {
                w.Close();
            }
        }

        private void OnLogout(object parameter)
        {
            int userId = _userSession.GetUserId();
            MessageBoxResult result = MessageBox.Show(
                "Ви впевнені, що хочете вийти з облікового запису?",
                "Підтвердження виходу",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _userSession.ClearSession();

                    _navigationService.ShowLogin();

                    List<Window> windowsToClose = Application.Current.Windows.Cast<Window>().ToList();

                    foreach (Window w in windowsToClose)
                    {
                        if (w != Application.Current.MainWindow)
                        {
                            w.Close();
                        }
                    }

                    _logger.LogInformation("Програма успішно перенаправлена на Login/Registration.", userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Помилка під час виходу користувача ID: {UserId}.", userId);
                    _ = MessageBox.Show($"Помилка виходу: {ex.Message}", "Помилка");
                }
            }
            else
            {
                _logger.LogInformation("Користувач ID: {UserId} скасував вихід.", userId);
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