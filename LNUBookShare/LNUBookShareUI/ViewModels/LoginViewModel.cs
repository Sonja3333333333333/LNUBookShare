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
            get => _email;
            set => SetProperty(ref _email, value);
        }

        // ❌ ВЛАСТИВІСТЬ 'Password' ВИДАЛЕНО

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // --- Команди ---
        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        // --- Конструктор ---
        public LoginViewModel(IMediator mediator, INavigationService navigationService)
        {
            _mediator = mediator;
            _navigationService = navigationService;

            // 2. ЗМІНЕНО: Команди тепер приймають 'object'
            LoginCommand = new RelayCommand<object>(async (param) => await LoginAsync(param));
            GoToRegisterCommand = new RelayCommand<object>(GoToRegister);
        }

        // --- Метод Входу ---
        private async Task LoginAsync(object parameter)
        {
            // 3. "Розпаковуємо" PasswordBox
            if (parameter is not PasswordBox passwordBox)
            {
                ErrorMessage = "Сталася помилка. Не вдалося отримати пароль.";
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

                LoginResultDto result = await _mediator.Send(query);

                if (result != null)
                {
                    // Успіх!
                    ErrorMessage = ""; // Очищуємо помилки
                    _navigationService.ShowMainView();

                    var window = GetWindowFromParameter(parameter);
                    window?.Close();
                }
            }
            catch (Exception ex)
            {
                // BLL кинув помилку (напр. "Невірний email або пароль")
                ErrorMessage = ex.Message;
            }
        }

        // --- Метод переходу на Реєстрацію ---
        private void GoToRegister(object parameter)
        {
            _navigationService.ShowRegister();

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