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

namespace LNUBookShareUI.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;

        private string _firstName;
        private string _lastName;
        private string _email;
        private FacultyDto _selectedFaculty;
        private string _errorMessage;

        public RegisterViewModel(IMediator mediator, INavigationService navigationService)
        {
            _mediator = mediator;
            _navigationService = navigationService;

            RegisterCommand = new RelayCommand<object>(async (param) => await RegisterAsync(param));
            GoToLoginCommand = new RelayCommand<object>(GoToLogin);

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

        public ObservableCollection<FacultyDto> Faculties { get; } = new();

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        private async Task RegisterAsync(object parameter)
        {
            if (parameter is not PasswordBox passwordBox)
            {
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
                    ErrorMessage = "Будь ласка, заповніть усі поля.";
                    return;
                }

                if (!Email.EndsWith("@lnu.edu.ua"))
                {
                    ErrorMessage = "Дозволено лише пошту @lnu.edu.ua.";
                    return;
                }

                if (password.Length < 9)
                {
                    ErrorMessage = "Пароль >= 9 символів.";
                    return;
                }

                var command = new RegisterUserCommand
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    Password = password,
                    FacultyId = SelectedFaculty.FacultyId
                };

                _ = await _mediator.Send(command);

                _ = MessageBox.Show("Перевірте пошту для підтвердження реєстрації.", "Реєстрація успішна",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                GoToLogin(parameter);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async Task LoadFacultiesAsync()
        {
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
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не вдалося завантажити список факультетів: " + ex.Message;
            }
        }

        private void GoToLogin(object parameter)
        {
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