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
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        // ... (додайте всі інші властивості, як у AddBookViewModel) ...
        private string _author;
        public string Author { get => _author; set => SetProperty(ref _author, value); }

        private string _isbn;
        public string Isbn { get => _isbn; set => SetProperty(ref _isbn, value); }

        private int? _year;
        public int? Year { get => _year; set => SetProperty(ref _year, value); }

        private string _publisher;
        public string Publisher { get => _publisher; set => SetProperty(ref _publisher, value); }

        private string _language;
        public string Language { get => _language; set => SetProperty(ref _language, value); }

        // --- Для обкладинки ---
        private string _coverImagePath;
        public string CoverImagePath { get => _coverImagePath; set => SetProperty(ref _coverImagePath, value); }

        // --- Для ComboBox Категорій ---
        public ObservableCollection<CategoryDto> Categories { get; } = new();
        private CategoryDto _selectedCategory;
        public CategoryDto SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }

        // --- Для RadioButton Статусу ---
        private string _status;
        public string Status { get => _status; set => SetProperty(ref _status, value); }

        // --- Команди ---
        public ICommand ChangeCoverCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditBookViewModel(IMediator mediator)
        {
            _mediator = mediator;
            ChangeCoverCommand = new RelayCommand(async () => await ChangeCover());
            SaveCommand = new RelayCommand<object>(async (w) => await Save(w));
            CancelCommand = new RelayCommand<object>(Cancel);
        }

        public async Task LoadDataAsync(int bookId)
        {
            _currentBookId = bookId; // Зберігаємо ID книги

            // 1. Завантажуємо категорії
            var categoryList = await _mediator.Send(new GetAllCategoriesQuery());
            Categories.Clear();
            foreach (var category in categoryList)
            {
                Categories.Add(category);
            }

            // 2. Завантажуємо дані книги
            var dto = await _mediator.Send(new GetBookForEditQuery { BookId = bookId, CurrentUserId = _currentUserId });

            Title = dto.Title;
            Author = dto.Author;
            Isbn = dto.Isbn;
            Year = dto.Year;
            Publisher = dto.Publisher;
            Language = dto.Language;
            Status = dto.Status;
            CoverImagePath = dto.CoverImagePath; // Вже абсолютний шлях
            SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == dto.CategoryId);
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

                    CoverImagePath = await _mediator.Send(uploadCommand);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося завантажити фото: {ex.Message}", "Помилка");
                }
            }
        }

        private async Task Save(object window)
        {
            try
            {
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

                await _mediator.Send(command);

                if (window is Window w) { w.Close(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося зберегти книгу: {ex.Message}", "Помилка");
            }
        }

        private void Cancel(object window)
        {
            if (window is Window w) { w.Close(); }
        }
    }
}