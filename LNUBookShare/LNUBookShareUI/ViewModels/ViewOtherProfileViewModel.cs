using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Profile;
using LNUBookShareDAL.Models;
using LNUBookShareUI.Common;
using LNUBookShareUI.Views;
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
using System.Windows.Navigation;


namespace LNUBookShareUI.ViewModels
{
    public class ViewOtherProfileViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly int _currentUserId ;
        private readonly INavigationService _navigationService;

        // --- Властивості для Інфо про Юзера ---
        private ProfileDto _profile;
        public ProfileDto Profile
        {
            get => this._profile;
            set => this.SetProperty(ref this._profile, value);
        }
        // ---- Властивість для видимості кнопок ----
        public bool IsMyProfile => false;

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
            get => this._selectedSort;
            set
            {
                if (this.SetProperty(ref this._selectedSort, value))
                {
                    this.ApplySort(); // Застосовуємо сортування
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
                    this.ApplyFilter(); // Застосовуємо фільтр
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

        public ICommand OpenBookDetailsCommand { get; }

        // --- Конструктор ---
        public ViewOtherProfileViewModel(IMediator mediator, INavigationService navigationService, int  userId)
        {
            this._mediator = mediator;
            this._currentUserId = userId;
            this._navigationService = navigationService;

            // Ініціалізуємо "розумний" список
            this.OwnedBooksView = CollectionViewSource.GetDefaultView(this._allOwnedBooks);
            this.OwnedBooksView.Filter = this.FilterBooks; // Прив'язуємо фільтр

            // Словник для сортування (як у MainViewModel)
            this.SortOptions = new Dictionary<BookSortCriteria, string>
            {
                { BookSortCriteria.Title, "Назва" },
                { BookSortCriteria.Author, "Автор" },
                { BookSortCriteria.Year, "Рік" }
            };

            // Команди
            this.LoadDataCommand = new RelayCommand(async () => await this.LoadProfileAsync());
            this.DeleteBookCommand = new RelayCommand<int>(async (bookId) => await this.DeleteBookAsync(bookId));

            this.SetFilterAllCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.All);
            this.SetFilterAvailableCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.Available);
            this.SetFilterIssuedCommand = new RelayCommand(() => this.SelectedStatusFilter = BookFilterStatus.Issued);

            this.GoBackCommand = new RelayCommand<object>(this.GoBack);

            this.OpenBookDetailsCommand = new RelayCommand<int>(this.OpenBookDetails);

            _ = this.LoadProfileAsync();
        }

        // --- Методи ---
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

                // 1. Заповнюємо дані профілю
                this.Profile = result;

                // 2. Заповнюємо повний список книг
                App.Current.Dispatcher.Invoke(() =>
                {
                    this._allOwnedBooks.Clear();
                    foreach (var book in result.OwnedBooks)
                    {
                        this._allOwnedBooks.Add(book);
                    }
                });

                // 3. Застосовуємо поточні сортування/фільтри
                this.ApplySort();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Помилка завантаження профілю: {ex.Message}\n\n{ex.StackTrace}");
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
                _ = await this._mediator.Send(command);
                // Успіх: видаляємо книгу з локального списку (швидше, ніж перезавантажувати)
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

        // --- Логіка Фільтрації та Сортування (локально) ---

        private void ApplyFilter()
        {
            // Просто "змушуємо" ICollectionView оновити свій фільтр
            this.OwnedBooksView.Refresh();
        }

        private bool FilterBooks(object item)
        {
            if (this.SelectedStatusFilter == BookFilterStatus.All)
            {
                return true; // Показуємо всі
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
            // 'window' - це параметр, який ми передаємо з XAML
            if (window is Window w)
            {
                w.Close();
            }
        }

        private void ApplySort()
        {
            // Очищуємо старі сортування
            this.OwnedBooksView.SortDescriptions.Clear();

            // Додаємо нове сортування
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
