using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Auth;
using LNUBookShareBLL.Features.Faculties;
using LNUBookShareUI.Common;
using MediatR;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; 
using System.Windows.Input;

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

    
        public string FirstName
        {
            get => this._firstName;
            set => this.SetProperty(ref this._firstName, value);
        }

        public string LastName
        {
            get => this._lastName;
            set => this.SetProperty(ref this._lastName, value);
        }

        public string Email
        {
            get => this._email;
            set => this.SetProperty(ref this._email, value);
        }

        public FacultyDto SelectedFaculty
        {
            get => this._selectedFaculty;
            set => this.SetProperty(ref this._selectedFaculty, value);
        }

        public string ErrorMessage
        {
            get => this._errorMessage;
            set => this.SetProperty(ref this._errorMessage, value);
        }

        public ObservableCollection<FacultyDto> Faculties { get; } = new();

        // --- Команди ---
        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        // --- Конструктор ---
        public RegisterViewModel(IMediator mediator, INavigationService navigationService)
        {
            this._mediator = mediator;
            this._navigationService = navigationService;

            // 👇 1. Команда ТЕПЕР ПРИЙМАЄ 'object'
            this.RegisterCommand = new RelayCommand<object>(async (param) => await this.RegisterAsync(param));
            this.GoToLoginCommand = new RelayCommand<object>(this.GoToLogin);

            _ = this.LoadFacultiesAsync();
        }

        // --- Метод реєстрації ---
 
        private async Task RegisterAsync(object parameter)
        {
         
    
            if (parameter is not PasswordBox passwordBox)
            {
                this.ErrorMessage = "Сталася помилка (PasswordBox == null).";
                return;
            }
            string password = passwordBox.Password; 

            try
            {
                if (string.IsNullOrWhiteSpace(this.FirstName) ||
                    string.IsNullOrWhiteSpace(this.LastName) ||
                    string.IsNullOrWhiteSpace(this.Email) ||
                    string.IsNullOrWhiteSpace(password) ||
                    this.SelectedFaculty == null)
                {
                    this.ErrorMessage = "Будь ласка, заповніть усі поля.";
                    return;
                }

                if (!this.Email.EndsWith("@lnu.edu.ua"))
                {
                    this.ErrorMessage = "Дозволено лише пошту @lnu.edu.ua.";
                    return;
                }

                if (password.Length < 9)
                {
                    this.ErrorMessage = "Пароль >= 9 символів.";
                    return;
                }

                var command = new RegisterUserCommand
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                    Email = this.Email,
                    Password = password,
                    FacultyId = this.SelectedFaculty.FacultyId
                };


                _ = await this._mediator.Send(command);

                _ = MessageBox.Show("Перевірте пошту для підтвердження реєстрації.", "Реєстрація успішна",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                this.GoToLogin(parameter);
            }
            catch (Exception ex)
            {

                this.ErrorMessage = ex.Message;
            }
        }

        private async Task LoadFacultiesAsync()
        {
            try
            {
                var faculties = await this._mediator.Send(new GetAllFacultiesQuery());
                App.Current.Dispatcher.Invoke(() =>
                {
                    this.Faculties.Clear();
                    foreach (var f in faculties)
                    {
                        this.Faculties.Add(f);
                    }
                });
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Не вдалося завантажити список факультетів: " + ex.Message;
            }
        }

     
        private void GoToLogin(object parameter)
        {
            this._navigationService.ShowLogin();

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