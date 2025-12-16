using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Categories;
using LNUBookShareBLL.Features.Files;

using LNUBookShareUI.Common;

using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace LNUBookShareUI.ViewModels
{
    public class AddBookViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSession _userSession;
        private readonly ILogger<AddBookViewModel> _logger;

        private string _title = string.Empty;
        private string _author = string.Empty;
        private string _isbn;
        private int? _year;
        private string _publisher;
        private string _language;
        private string _coverImagePath;
        private CategoryDto _selectedCategory;

        public AddBookViewModel(IMediator mediator, IUserSession userSession, ILogger<AddBookViewModel> logger)
        {
            _mediator = mediator;
            _userSession = userSession;
            _logger = logger;

            ChangeCoverCommand = new RelayCommand(async () => await ChangeCover());
            SaveCommand = new RelayCommand<object>(async (w) => await Save(w));
            CancelCommand = new RelayCommand<object>(Cancel);

            _logger.LogInformation("AddBookViewModel ініціалізовано.");
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        public string Isbn
        {
            get => _isbn;
            set => SetProperty(ref _isbn, value);
        }

        public int? Year
        {
            get => _year;
            set => SetProperty(ref _year, value);
        }

        public string Publisher
        {
            get => _publisher;
            set => SetProperty(ref _publisher, value);
        }

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public string CoverImagePath
        {
            get => _coverImagePath;
            set => SetProperty(ref _coverImagePath, value);
        }

        public ObservableCollection<CategoryDto> Categories { get; } = new ();

        public CategoryDto SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ICommand ChangeCoverCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public async Task LoadDataAsync()
        {
            try
            {
                _logger.LogInformation("Завантаження списку категорій...");
                var categoryList = await _mediator.Send(new GetAllCategoriesQuery());
                Categories.Clear();
                foreach (var category in categoryList)
                {
                    Categories.Add(category);
                }

                SelectedCategory = Categories.FirstOrDefault();
                _logger.LogInformation("Успішно завантажено {Count} категорій.", Categories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при завантаженні категорій.");
            }
        }

        private async Task ChangeCover()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    _logger.LogInformation("Користувач обрав файл для обкладинки: {FilePath}", filePath);

                    byte[] imageData = File.ReadAllBytes(filePath);

                    var uploadCommand = new UploadImageCommand
                    {
                        FileName = Path.GetFileName(filePath),
                        ImageData = imageData,
                    };

                    string newPhysicalPath = await _mediator.Send(uploadCommand);
                    CoverImagePath = newPhysicalPath;
                    _logger.LogInformation("Обкладинку успішно завантажено на сервер: {NewPath}", newPhysicalPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Не вдалося завантажити фото обкладинки.");
                    _ = MessageBox.Show($"Не вдалося завантажити фото: {ex.Message}", "Помилка");
                }
            }
            else
            {
                _logger.LogInformation("Користувач скасував вибір фото.");
            }
        }

        private async Task Save(object window)
        {
            _logger.LogInformation("Спроба збереження книги: '{Title}' (Автор: {Author})", Title, Author);

            try
            {
                if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Author) || SelectedCategory == null)
                {
                    _logger.LogWarning("Валідація не пройшла: відсутні обов'язкові поля.");
                    throw new Exception("Назва, Автор та Категорія є обов'язковими.");
                }

                var dto = new AddBookDto
                {
                    Title = Title,
                    Author = Author,
                    Isbn = Isbn,
                    Year = Year,
                    Publisher = Publisher,
                    Language = Language,
                    CategoryId = SelectedCategory.CategoryId,
                    CoverImagePath = CoverImagePath,
                };

                var command = new AddBookCommand
                {
                    Dto = dto,
                    OwnerUserId = _userSession.GetUserId(),
                };

                _ = await _mediator.Send(command);

                _logger.LogInformation("Книгу '{Title}' успішно додано до бази даних.", Title);

                if (window is Window w)
                {
                    w.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при збереженні книги '{Title}'.", Title);
                _ = MessageBox.Show($"Не вдалося зберегти книгу: {ex.Message}", "Помилка");
            }
        }

        private void Cancel(object window)
        {
            _logger.LogInformation("Користувач скасував додавання книги.");
            if (window is Window w)
            {
                w.Close();
            }
        }
    }
}