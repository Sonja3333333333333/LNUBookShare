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
using System.Windows.Data; // Потрібен для ICollectionView
using System.Windows.Input;
using System.Windows.Navigation;
using LNUBookShareBLL.DTOs;
using LNUBookShareUI.Views;

namespace LNUBookShareUI.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly int _currentUserId = 1; 
        private readonly INavigationService _navigationService;

        // --- Властивості для Інфо про Юзера ---
        private ProfileDto _profile;
        public ProfileDto Profile
        {
            get => _profile;
            set => SetProperty(ref _profile, value);
        }

        private bool _isMyProfile;
        public bool IsMyProfile
        {
            get => _isMyProfile;
            set
            {
                _isMyProfile = value;
                OnPropertyChanged(nameof(IsMyProfile)); 
            }
        }

        // --- Властивості для Списку Книг ---
        // Повний (нефільтрований) список книг
        private ObservableCollection<OwnedBookDto> _allOwnedBooks = new();

        // "Розумний" список, який бачить UI (з фільтрами)
        public ICollectionView OwnedBooksView { get; }

        // --- Властивості для Фільтрів та Сортування ---
        public Dictionary<BookSortCriteria, string> SortOptions { get; }
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        public BookSortCriteria SelectedSort
        {
            get => _selectedSort;
            set
            {
                if (SetProperty(ref _selectedSort, value))
                {
                    ApplySort(); // Застосовуємо сортування
                }
            }
        }

        private BookFilterStatus _selectedStatusFilter = BookFilterStatus.All;
        public BookFilterStatus SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value))
                {
                    ApplyFilter(); // Застосовуємо фільтр
                }
            }
        }
        

        // --- Команди ---
        public ICommand LoadDataCommand { get; }
        public ICommand DeleteBookCommand { get; }
        public ICommand SetFilterAllCommand { get; }
        public ICommand SetFilterAvailableCommand { get; }
        public ICommand SetFilterIssuedCommand { get; }

        public ICommand GoBackCommand { get; }
        public ICommand OpenEditProfileCommand { get; }


        public ICommand OpenBookDetailsCommand { get; }

        // --- Конструктор ---
        public ProfileViewModel(IMediator mediator, INavigationService navigationService)
        {
            _mediator = mediator;
            this.IsMyProfile = true;
            _navigationService = navigationService;

            // Ініціалізуємо "розумний" список
            OwnedBooksView = CollectionViewSource.GetDefaultView(_allOwnedBooks);
            OwnedBooksView.Filter = FilterBooks; // Прив'язуємо фільтр

            // Словник для сортування (як у MainViewModel)
            SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" }
            };

            // Команди
            LoadDataCommand = new RelayCommand(async () => await LoadProfileAsync());
            DeleteBookCommand = new RelayCommand<int>(async (bookId) => await DeleteBookAsync(bookId));

            SetFilterAllCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.All);
            SetFilterAvailableCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.Available);
            SetFilterIssuedCommand = new RelayCommand(() => SelectedStatusFilter = BookFilterStatus.Issued);

            GoBackCommand = new RelayCommand<object>(GoBack);

            OpenBookDetailsCommand = new RelayCommand<int>(OpenBookDetails);

            // Завантажуємо дані при відкритті
            _ = LoadProfileAsync();

            _navigationService = navigationService;
            OpenEditProfileCommand = new RelayCommand(OpenEditProfile);
        }

        private void OpenEditProfile()
        {
            _navigationService.ShowEditProfile();
        }


        // --- Методи ---
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
                var query = new GetProfileQuery { UserId = _currentUserId };
                var result = await _mediator.Send(query);

                // 1. Заповнюємо дані профілю
                Profile = result;

                // 2. Заповнюємо повний список книг
                App.Current.Dispatcher.Invoke(() =>
                {
                    _allOwnedBooks.Clear();
                    foreach (var book in result.OwnedBooks)
                    {
                        _allOwnedBooks.Add(book);
                    }
                });

                // 3. Застосовуємо поточні сортування/фільтри
                ApplySort();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження профілю: {ex.Message}");
            }
        }

        private async Task DeleteBookAsync(int bookId)
        {
            // TODO: Показати кастомний MessageBox "Ви впевнені?"
            var command = new DeleteBookCommand
            {
                BookId = bookId,
                CurrentUserId = _currentUserId
            };

            try
            {
                await _mediator.Send(command);
                // Успіх: видаляємо книгу з локального списку (швидше, ніж перезавантажувати)
                var bookToRemove = _allOwnedBooks.FirstOrDefault(b => b.BookId == bookId);
                if (bookToRemove != null)
                {
                    _allOwnedBooks.Remove(bookToRemove);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення: {ex.Message}");
            }
        }

        // --- Логіка Фільтрації та Сортування (локально) ---

        private void ApplyFilter()
        {
            // Просто "змушуємо" ICollectionView оновити свій фільтр
            OwnedBooksView.Refresh();
        }

        private bool FilterBooks(object item)
        {
            if (SelectedStatusFilter == BookFilterStatus.All)
            {
                return true; // Показуємо всі
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
            // Очищуємо старі сортування
            OwnedBooksView.SortDescriptions.Clear();

            // Додаємо нове сортування
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
