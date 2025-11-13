using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Auth;
using LNUBookShareUI.Common;
using MediatR;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // 👈 1. ДОДАНО ДЛЯ PASSWORD BOX
using System.Windows.Input;

namespace LNUBookShareUI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;

        // --- Властивості ---
        private string _email;
        public string Email
        {
            get => this._email;
            set => this.SetProperty(ref this._email, value);
        }

        // ❌ ВЛАСТИВІСТЬ 'Password' ВИДАЛЕНО

        private string _errorMessage;
        public string ErrorMessage
        {
            get => this._errorMessage;
            set => this.SetProperty(ref this._errorMessage, value);
        }

        // --- Команди ---
        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        // --- Конструктор ---
        public LoginViewModel(IMediator mediator, INavigationService navigationService)
        {
            this._mediator = mediator;
            this._navigationService = navigationService;

            this.LoginCommand = new RelayCommand<object>(async (param) => await this.LoginAsync(param));
            this.GoToRegisterCommand = new RelayCommand<object>(this.GoToRegister);
        }

        // --- Метод Входу ---
        private async Task LoginAsync(object parameter)
        {
            // 3. "Розпаковуємо" PasswordBox
            if (parameter is not PasswordBox passwordBox)
            {
                this.ErrorMessage = "Сталася помилка. Не вдалося отримати пароль.";
                return;
            }

            // 4. Отримуємо пароль звідси
            string password = passwordBox.Password;

            try
            {
                // BLL тепер отримає справжній пароль
                var query = new LoginUserQuery
                {
                    Email = this.Email,
                    Password = password
                };

                LoginResultDto result = await this._mediator.Send(query);

                if (result != null)
                {
                    // Успіх!
                    this.ErrorMessage = ""; // Очищуємо помилки
                    this._navigationService.ShowMainView();

                    var window = this.GetWindowFromParameter(parameter);
                    window?.Close();
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        // --- Метод переходу на Реєстрацію ---
        private void GoToRegister(object parameter)
        {
            this._navigationService.ShowRegister();

            // Закриваємо поточне вікно (LoginView)
            if (parameter is Window w)
            {
                w.Close();
            }
        }

        private Window GetWindowFromParameter(object parameter)
        {
            if (parameter is FrameworkElement element)
            {
                return Window.GetWindow(element);
            }
            return null;
        }
    }
}