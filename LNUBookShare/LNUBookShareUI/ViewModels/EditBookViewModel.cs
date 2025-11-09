using LNUBookShareUI.Common;
using MediatR;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LNUBookShareUI.ViewModels
{
    public class EditBookViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;

        public int BookId { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _author;
        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        private string _isbn;
        public string ISBN
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

        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>
        {
            "Математика",
            "Фізика",
            "Програмування",
            "Історія",
            "Література",
            "Інше"
        };

        private string _status;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string _coverPath;
        public string CoverPath
        {
            get => _coverPath;
            set => SetProperty(ref _coverPath, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ChangePhotoCommand { get; }

        public EditBookViewModel(IMediator mediator, INavigationService navigationService)
        {
            _mediator = mediator;
            _navigationService = navigationService;

            SaveCommand = new RelayCommand<object>(async (window) => await SaveBookAsync(window));
            CancelCommand = new RelayCommand<object>(Cancel);
            ChangePhotoCommand = new RelayCommand(ChangePhoto);
        }

        public async Task LoadBookDataAsync()
        {
            try
            {
                // TODO: Підключити BLL - GetBookForEditQuery

                // Тимчасові тестові дані
                Title = "Приклад книги";
                Author = "Автор Тестовий";
                ISBN = "978-0-123456-78-9";
                Year = 2020;
                Publisher = "Видавництво ЛНУ";
                Language = "Українська";
                SelectedCategory = "Програмування";
                Status = "available";
                CoverPath = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка");
            }
        }

        private void ChangePhoto()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Зображення|*.jpg;*.jpeg;*.png;*.bmp",
                    Title = "Виберіть обкладинку книги"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    // Копіюємо файл в папку проекту або зберігаємо шлях
                    CoverPath = openFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка вибору фото: {ex.Message}", "Помилка");
            }
        }

        private async Task SaveBookAsync(object windowParameter)
        {
            try
            {
                // Валідація
                if (string.IsNullOrWhiteSpace(Title))
                {
                    MessageBox.Show("Введіть назву книги!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Author))
                {
                    MessageBox.Show("Введіть автора книги!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedCategory == null)
                {
                    MessageBox.Show("Оберіть категорію!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // TODO: Підключити BLL - UpdateBookCommand

                MessageBox.Show("Книга збережена! (Тимчасова заглушка)", "Успіх");

                // Закриваємо вікно
                if (windowParameter is Window window)
                {
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка");
            }
        }

        private void Cancel(object windowParameter)
        {
            if (windowParameter is Window window)
            {
                window.Close();
            }
        }
    }
}