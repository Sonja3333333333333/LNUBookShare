
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Auth;
using LNUBookShareBLL.Features.Faculties;
using LNUBookShareUI.Common;
using MediatR;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // 👈 Потрібно для PasswordBox
using System.Windows.Input;

namespace LNUBookShareUI.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService; // 👈 1. ДОДАНО СЕРВІС

        // --- Властивості ---
        private string _firstName;
        private string _lastName;
        private string _email;
        private FacultyDto _selectedFaculty;
        private string _errorMessage; // Для показу помилок

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public FacultyDto SelectedFaculty
        {
            get => _selectedFaculty;
            set => SetProperty(ref _selectedFaculty, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ObservableCollection<FacultyDto> Faculties { get; } = new();

        // --- Команди ---
        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        // --- Конструктор ---
        // 👇 2. СЕРВІС ДОДАНО У КОНСТРУКТОР
        public RegisterViewModel(IMediator mediator, INavigationService navigationService)
        {
            _mediator = mediator;
            _navigationService = navigationService; // 👈 ЗБЕРЕЖЕНО

            // 👇 3. ОНОВЛЕНО КОМАНДИ
            RegisterCommand = new RelayCommand<object>(async (param) => await RegisterAsync(param));
            GoToLoginCommand = new RelayCommand<object>(GoToLogin); // 👈 Тепер приймає 'object'

            _ = LoadFacultiesAsync();
        }

        // --- Метод реєстрації ---
        // 👇 4. ОНОВЛЕНО МЕТОД (приймає PasswordBox)
        private async Task RegisterAsync(object parameter)
        {
            if (parameter is not PasswordBox passwordBox)
            {
                ErrorMessage = "Сталася помилка. Не вдалося отримати пароль.";
                return;
            }
            string password = passwordBox.Password;

            try
            {
                if (string.IsNullOrWhiteSpace(FirstName) ||
                    string.IsNullOrWhiteSpace(LastName) ||
                    string.IsNullOrWhiteSpace(Email) ||
                    string.IsNullOrWhiteSpace(password) ||
                    SelectedFaculty == null)
                {
                    ErrorMessage = "Будь ласка, заповніть усі поля.";
                    return;
                }

                var command = new RegisterUserCommand
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                    Email = this.Email,
                    Password = password, // 👈 Використовуємо безпечний пароль
                    FacultyId = this.SelectedFaculty.FacultyId
                };

                await _mediator.Send(command);

                ErrorMessage = ""; // Очистити помилки

                MessageBox.Show("Перевірте пошту для підтвердження реєстрації.", "Реєстрація успішна",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                // 👇 5. ОНОВЛЕНО (Викликаємо той самий метод, що й кнопка)
                GoToLogin(passwordBox);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        // --- Завантаження факультетів у ComboBox ---
        private async Task LoadFacultiesAsync()
        {
            try
            {
                var faculties = await _mediator.Send(new GetAllFacultiesQuery());

                App.Current.Dispatcher.Invoke(() =>
                {
                    Faculties.Clear();
                    foreach (var f in faculties)
                        Faculties.Add(f);
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не вдалося завантажити список факультетів: " + ex.Message;
            }
        }

        // --- Перехід назад до LoginView ---
        // 👇 6. ОНОВЛЕНО МЕТОД (знаходить вікно і закриває)
        private void GoToLogin(object parameter)
        {
            // Використовуємо наш сервіс навігації!
            _navigationService.ShowLogin();

            // Закриваємо поточне вікно (RegisterView)
            Window windowToClose = null;
            if (parameter is Window w)
            {
                windowToClose = w;
            }
            else if (parameter is FrameworkElement element)
            {
                windowToClose = Window.GetWindow(element);
            }

            windowToClose?.Close();
        }
    }
}