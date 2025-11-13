using MediatR;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Categories;
using LNUBookShareBLL.Features.Files;
using LNUBookShareUI.Common;
using System.Linq;
using System;

namespace LNUBookShareUI.ViewModels
{
    public class EditBookViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private int _currentBookId;
        private int _currentUserId = 1; // "Захардкоджено"

        // --- Властивості для полів вводу ---
        private string _title = string.Empty;
        public string Title { get => this._title; set => this.SetProperty(ref this._title, value); }
        // ... (додайте всі інші властивості, як у AddBookViewModel) ...
        private string _author;
        public string Author { get => this._author; set => this.SetProperty(ref this._author, value); }

        private string _isbn;
        public string Isbn { get => this._isbn; set => this.SetProperty(ref this._isbn, value); }

        private int? _year;
        public int? Year { get => this._year; set => this.SetProperty(ref this._year, value); }

        private string _publisher;
        public string Publisher { get => this._publisher; set => this.SetProperty(ref this._publisher, value); }

        private string _language;
        public string Language { get => this._language; set => this.SetProperty(ref this._language, value); }

        // --- Для обкладинки ---
        private string _coverImagePath;
        public string CoverImagePath { get => this._coverImagePath; set => this.SetProperty(ref this._coverImagePath, value); }

        // --- Для ComboBox Категорій ---
        public ObservableCollection<CategoryDto> Categories { get; } = new();
        private CategoryDto _selectedCategory;
        public CategoryDto SelectedCategory { get => this._selectedCategory; set => this.SetProperty(ref this._selectedCategory, value); }

        // --- Для RadioButton Статусу ---
        private string _status;
        public string Status { get => this._status; set => this.SetProperty(ref this._status, value); }

        // --- Команди ---
        public ICommand ChangeCoverCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditBookViewModel(IMediator mediator)
        {
            this._mediator = mediator;
            this.ChangeCoverCommand = new RelayCommand(async () => await this.ChangeCover());
            this.SaveCommand = new RelayCommand<object>(async (w) => await this.Save(w));
            this.CancelCommand = new RelayCommand<object>(this.Cancel);
        }

        public async Task LoadDataAsync(int bookId)
        {
            this._currentBookId = bookId; // Зберігаємо ID книги

            // 1. Завантажуємо категорії
            var categoryList = await this._mediator.Send(new GetAllCategoriesQuery());
            this.Categories.Clear();
            foreach (var category in categoryList)
            {
                this.Categories.Add(category);
            }

            // 2. Завантажуємо дані книги
            var dto = await this._mediator.Send(new GetBookForEditQuery { BookId = bookId, CurrentUserId = _currentUserId });

            this.Title = dto.Title;
            this.Author = dto.Author;
            this.Isbn = dto.Isbn;
            this.Year = dto.Year;
            this.Publisher = dto.Publisher;
            this.Language = dto.Language;
            this.Status = dto.Status;
            this.CoverImagePath = dto.CoverImagePath; // Вже абсолютний шлях
            this.SelectedCategory = this.Categories.FirstOrDefault(c => c.CategoryId == dto.CategoryId);
        }

        private async Task ChangeCover()
        {
            // (Цей код ідентичний тому, що у AddBookViewModel)
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    byte[] imageData = File.ReadAllBytes(filePath);

                    var uploadCommand = new UploadImageCommand
                    {
                        FileName = Path.GetFileName(filePath),
                        ImageData = imageData
                    };

                    this.CoverImagePath = await this._mediator.Send(uploadCommand);
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Не вдалося завантажити фото: {ex.Message}", "Помилка");
                }
            }
        }

        private async Task Save(object window)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.Title) ||
                    string.IsNullOrWhiteSpace(this.Author) ||
                    this.SelectedCategory == null)
                {
                    // Використовуємо MessageBox, оскільки це UI
                    _ = MessageBox.Show("Поля 'Назва', 'Автор' та 'Категорія' є обов'язковими.",
                                    "Помилка валідації",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return; 
                }


                var dto = new BookEditDto
                {
                    Title = this.Title,
                    Author = this.Author,
                    Isbn = this.Isbn,
                    Year = this.Year,
                    Publisher = this.Publisher,
                    Language = this.Language,
                    CategoryId = this.SelectedCategory.CategoryId,
                    Status = this.Status,
                    CoverImagePath = this.CoverImagePath
                };

                var command = new UpdateBookCommand
                {
                    BookId = _currentBookId,
                    CurrentUserId = _currentUserId,
                    Dto = dto
                };

                _ = await this._mediator.Send(command);

                if (window is Window w) { w.Close(); }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Не вдалося зберегти книгу: {ex.Message}", "Помилка");
            }
        }

        private void Cancel(object window)
        {
            if (window is Window w) { w.Close(); }
        }
    }
}