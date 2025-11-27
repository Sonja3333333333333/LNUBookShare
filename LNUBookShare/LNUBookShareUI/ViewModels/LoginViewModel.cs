//(DRY/SRP):

//Видалено старий метод GetWindowFromParameter.
//Створено єдиний приватний метод TryCloseWindow(object parameter), який інкапсулює логіку знаходження та закриття вікна (яке може бути передане як Window або FrameworkElement).
//Це усунуло дублювання коду закриття в LoginAsync та GoToRegister.

//(SRP):

//Створено приватний метод HandleSuccessfulLogin(LoginResultDto result, object parameter), який тепер відповідає лише за дії після успішної автентифікації (оновлення сесії, навігація та закриття вікна).
//Це робить метод LoginAsync чистішим і сфокусованим лише на виклику IMediator та обробці результатів/помилок.

//Покращена обробка помилок:

//Додано перевірку, що обєкт result не є null після виклику _mediator.Send(), і встановлення повідомлення про помилку, якщо вхід не вдався.




using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Auth;
using LNUBookShareUI.Common;
using MediatR;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LNUBookShareUI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;

        private string _email;
        public string Email
        {
            get => this._email;
            set => this.SetProperty(ref this._email, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => this._errorMessage;
            set => this.SetProperty(ref this._errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        public LoginViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession)
        {
            this._mediator = mediator;
            this._navigationService = navigationService;
            this._userSession = userSession;

            this.LoginCommand = new RelayCommand<object>(async (param) => await this.LoginAsync(param));
            this.GoToRegisterCommand = new RelayCommand<object>(this.GoToRegister);
        }

        private void TryCloseWindow(object parameter)
        {
            Window window = null;

            if (parameter is Window w)
            {
                window = w;
            }
            else if (parameter is FrameworkElement element)
            {
                window = Window.GetWindow(element);
            }

            window?.Close();
        }

        private void HandleSuccessfulLogin(LoginResultDto result, object parameter)
        {
            this.ErrorMessage = "";
            this._userSession.CurrentUser = result;
            this._navigationService.ShowMainView();

            this.TryCloseWindow(parameter);
        }

        private async Task LoginAsync(object parameter)
        {
            if (parameter is not PasswordBox passwordBox)
            {
                this.ErrorMessage = "Сталася помилка. Не вдалося отримати пароль.";
                return;
            }

            string password = passwordBox.Password;

            try
            {
                this.ErrorMessage = "";

                var query = new LoginUserQuery
                {
                    Email = this.Email,
                    Password = password
                };

                LoginResultDto result = await this._mediator.Send(query);

                if (result != null)
                {
                    this.HandleSuccessfulLogin(result, parameter);
                }
                else
                {
                    this.ErrorMessage = "Невірний email або пароль.";
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        private void GoToRegister(object parameter)
        {
            this._navigationService.ShowRegister();
            this.TryCloseWindow(parameter);
        }
    }
}