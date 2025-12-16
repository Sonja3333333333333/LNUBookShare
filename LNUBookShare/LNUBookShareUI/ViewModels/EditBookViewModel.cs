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
    public class EditBookViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSession _userSession;
        private readonly ILogger<EditBookViewModel> _logger;

        private int _currentBookId;

        private string _title = string.Empty;
        private string _author;
        private string _isbn;
        private int? _year;
        private string _publisher;
        private string _language;
        private string _coverImagePath;
        private CategoryDto _selectedCategory;
        private string _status;

        public EditBookViewModel(IMediator mediator, IUserSession userSession, ILogger<EditBookViewModel> logger)
        {
            _mediator = mediator;
            _userSession = userSession;
            _logger = logger;

            ChangeCoverCommand = new RelayCommand(async () => await ChangeCover());
            SaveCommand = new RelayCommand<object>(async (w) => await Save(w));
            CancelCommand = new RelayCommand<object>(Cancel);

            _logger.LogInformation("EditBookViewModel ініціалізовано.");
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

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public ICommand ChangeCoverCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public async Task LoadDataAsync(int bookId)
        {
            _currentBookId = bookId;
            _logger.LogInformation("Початок завантаження даних для редагування книги ID: {BookId}", bookId);

            try
            {
                var categoryList = await _mediator.Send(new GetAllCategoriesQuery());
                Categories.Clear();
                foreach (var category in categoryList)
                {
                    Categories.Add(category);
                }

                var dto = await _mediator.Send(new GetBookForEditQuery { BookId = bookId, CurrentUserId = _userSession.GetUserId() });

                Title = dto.Title;
                Author = dto.Author;
                Isbn = dto.Isbn;
                Year = dto.Year;
                Publisher = dto.Publisher;
                Language = dto.Language;
                Status = dto.Status;
                CoverImagePath = dto.CoverImagePath;
                SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == dto.CategoryId);

                _logger.LogInformation("Дані книги '{Title}' успішно завантажено.", Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при завантаженні даних книги ID: {BookId}", bookId);
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
                    byte[] imageData = File.ReadAllBytes(filePath);

                    var uploadCommand = new UploadImageCommand
                    {
                        FileName = Path.GetFileName(filePath),
                        ImageData = imageData,
                    };

                    CoverImagePath = await _mediator.Send(uploadCommand);
                    _logger.LogInformation("Нову обкладинку завантажено на сервер: {Path}", CoverImagePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Не вдалося завантажити нове фото обкладинки.");
                    _ = MessageBox.Show($"Не вдалося завантажити фото: {ex.Message}", "Помилка");
                }
            }
            else
            {
                _logger.LogInformation("Користувач скасував зміну обкладинки.");
            }
        }

        private async Task Save(object window)
        {
            _logger.LogInformation("Спроба збереження змін для книги ID: {BookId}", _currentBookId);

            try
            {
                if (string.IsNullOrWhiteSpace(Title) ||
                    string.IsNullOrWhiteSpace(Author) ||
                    SelectedCategory == null)
                {
                    _logger.LogWarning("Валідація не пройшла при редагуванні: відсутні обов'язкові поля.");

                    _ = MessageBox.Show(
                        "Поля 'Назва', 'Автор' та 'Категорія' є обов'язковими.",
                        "Помилка валідації",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var dto = new BookEditDto
                {
                    Title = Title,
                    Author = Author,
                    Isbn = Isbn,
                    Year = Year,
                    Publisher = Publisher,
                    Language = Language,
                    CategoryId = SelectedCategory.CategoryId,
                    Status = Status,
                    CoverImagePath = CoverImagePath,
                };

                var command = new UpdateBookCommand
                {
                    BookId = _currentBookId,
                    CurrentUserId = _userSession.GetUserId(),
                    Dto = dto,
                };

                _ = await _mediator.Send(command);

                _logger.LogInformation("Книгу ID: {BookId} успішно оновлено.", _currentBookId);

                if (window is Window w)
                {
                    w.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при збереженні змін книги ID: {BookId}", _currentBookId);
                _ = MessageBox.Show($"Не вдалося зберегти книгу: {ex.Message}", "Помилка");
            }
        }

        private void Cancel(object window)
        {
            _logger.LogInformation("Користувач скасував редагування книги.");
            if (window is Window w)
            {
                w.Close();
            }
        }
    }
}