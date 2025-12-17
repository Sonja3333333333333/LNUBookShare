using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Auth;
using LNUBookShareBLL.Features.Faculties;

using LNUBookShareUI.Common;

using MediatR;
using Microsoft.Extensions.Logging;

namespace LNUBookShareUI.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly ILogger<RegisterViewModel> _logger;

        private string _firstName;
        private string _lastName;
        private string _email;
        private FacultyDto _selectedFaculty;
        private string _errorMessage;

        public RegisterViewModel(IMediator mediator, INavigationService navigationService, ILogger<RegisterViewModel> logger)
        {
            _mediator = mediator;
            _navigationService = navigationService;
            _logger = logger;

            RegisterCommand = new RelayCommand<object>(async (param) => await RegisterAsync(param));
            GoToLoginCommand = new RelayCommand<object>(GoToLogin);

            _logger.LogInformation("RegisterViewModel ініціалізовано. Відкрито форму реєстрації.");

            _ = LoadFacultiesAsync();
        }

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

        public ObservableCollection<FacultyDto> Faculties { get; } = new ();

        public ICommand RegisterCommand { get; }

        public ICommand GoToLoginCommand { get; }

        private async Task RegisterAsync(object parameter)
        {
            _logger.LogInformation("Користувач ініціював спробу реєстрації. Email: {Email}", Email);

            if (parameter is not PasswordBox passwordBox)
            {
                _logger.LogWarning("Помилка UI: Не вдалося отримати доступ до PasswordBox.");
                ErrorMessage = "Сталася помилка (PasswordBox == null).";
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
                    _logger.LogWarning("Невдала спроба реєстрації (Email: {Email}): Не всі поля заповнені.", Email);
                    ErrorMessage = "Будь ласка, заповніть усі поля.";
                    return;
                }

                if (!Email.EndsWith("@lnu.edu.ua"))
                {
                    _logger.LogWarning("Невдала спроба реєстрації: Недопустимий домен пошти ({Email}).", Email);
                    ErrorMessage = "Дозволено лише пошту @lnu.edu.ua.";
                    return;
                }

                if (password.Length < 9)
                {
                    _logger.LogWarning("Невдала спроба реєстрації (Email: {Email}): Пароль занадто короткий.", Email);
                    ErrorMessage = "Пароль >= 9 символів.";
                    return;
                }

                var command = new RegisterUserCommand
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    Password = password,
                    FacultyId = SelectedFaculty.FacultyId,
                };

                _logger.LogInformation("Відправка запиту на реєстрацію для {Email}...", Email);

                _ = await _mediator.Send(command);

                _logger.LogInformation("Реєстрація успішна для {Email}. Показано повідомлення про підтвердження пошти.", Email);

                _ = MessageBox.Show("Перевірте пошту для підтвердження реєстрації.", "Реєстрація успішна", MessageBoxButton.OK, MessageBoxImage.Information);

                GoToLogin(parameter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критична помилка при реєстрації користувача {Email}.", Email);
                ErrorMessage = ex.Message;
            }
        }

        private async Task LoadFacultiesAsync()
        {
            _logger.LogInformation("Завантаження списку факультетів для форми реєстрації...");
            try
            {
                var faculties = await _mediator.Send(new GetAllFacultiesQuery());
                App.Current.Dispatcher.Invoke(() =>
                {
                    Faculties.Clear();
                    foreach (var f in faculties)
                    {
                        Faculties.Add(f);
                    }
                });
                _logger.LogInformation("Список факультетів успішно завантажено ({Count} записів).", Faculties.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не вдалося завантажити список факультетів.");
                ErrorMessage = "Не вдалося завантажити список факультетів: " + ex.Message;
            }
        }

        private void GoToLogin(object parameter)
        {
            _logger.LogInformation("Перехід на сторінку входу.");
            _navigationService.ShowLogin();

            Window? windowToClose = null;
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