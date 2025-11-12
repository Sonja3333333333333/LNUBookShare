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
    public class AddBookViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private int _currentUserId = 1; // "Захардкоджено" ID власника

        // --- Властивості для полів вводу ---
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _author = string.Empty;
        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        private string _isbn;
        public string Isbn
        {
            get => _isbn;
            set => SetProperty(ref _isbn, value);
        }

        private int? _year;
        public int? Year
        {
            get => _year;
            set => SetProperty(ref _year, value);
        }

        private string _publisher;
        public string Publisher
        {
            get => _publisher;
            set => SetProperty(ref _publisher, value);
        }

        private string _language;
        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        // --- Для обкладинки ---
        private string _coverImagePath;
        public string CoverImagePath
        {
            get => _coverImagePath;
            set => SetProperty(ref _coverImagePath, value);
        }

        // --- Для ComboBox ---
        public ObservableCollection<CategoryDto> Categories { get; } = new();
        private CategoryDto _selectedCategory;
        public CategoryDto SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        // --- Команди ---
        public ICommand ChangeCoverCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddBookViewModel(IMediator mediator)
        {
            _mediator = mediator;
            ChangeCoverCommand = new RelayCommand(async () => await ChangeCover());
            SaveCommand = new RelayCommand<object>(async (w) => await Save(w));
            CancelCommand = new RelayCommand<object>(Cancel);
        }

        public async Task LoadDataAsync()
        {
            // Завантажуємо категорії для ComboBox
            var categoryList = await _mediator.Send(new GetAllCategoriesQuery());
            Categories.Clear();
            foreach (var category in categoryList)
            {
                Categories.Add(category);
            }
            SelectedCategory = Categories.FirstOrDefault();
        }

        private async Task ChangeCover()
        {
            // Та сама логіка, що й для фото профілю
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

                    // BLL повертає АБСОЛЮТНИЙ шлях (C:\...)
                    string newPhysicalPath = await _mediator.Send(uploadCommand);
                    CoverImagePath = newPhysicalPath; // Оновлюємо UI
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
                // Валідація
                if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Author) || SelectedCategory == null)
                {
                    throw new Exception("Назва, Автор та Категорія є обов'язковими.");
                }

                // Створюємо DTO для BLL
                var dto = new AddBookDto
                {
                    Title = this.Title,
                    Author = this.Author,
                    Isbn = this.Isbn,
                    Year = this.Year,
                    Publisher = this.Publisher,
                    Language = this.Language,
                    CategoryId = this.SelectedCategory.CategoryId,
                    // Передаємо шлях до обкладинки, BLL сам розбереться з ID
                    CoverImagePath = this.CoverImagePath
                };

                // Створюємо команду
                var command = new AddBookCommand
                {
                    Dto = dto,
                    OwnerUserId = _currentUserId
                };

                // Відправляємо
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
