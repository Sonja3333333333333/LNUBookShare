using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Auth;

using LNUBookShareUI.Common;

using MediatR;

using Microsoft.Extensions.Logging;

namespace LNUBookShareUI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;
        private readonly ILogger<LoginViewModel> _logger;

        private string _email;
        private string _errorMessage;

        public LoginViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession, ILogger<LoginViewModel> logger)
        {
            _mediator = mediator;
            _navigationService = navigationService;
            _userSession = userSession;
            _logger = logger;

            LoginCommand = new RelayCommand<object>(async (param) => await LoginAsync(param));
            GoToRegisterCommand = new RelayCommand<object>(GoToRegister);
            _logger.LogInformation("LoginViewModel ініціалізовано.");
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }

        public ICommand GoToRegisterCommand { get; }

        private async Task LoginAsync(object parameter)
        {
            IsLoading = true;
            _logger.LogInformation("Користувач {Email} намагається увійти в систему.", Email);

            try
            {
                if (parameter is not PasswordBox passwordBox)
                {
                    _logger.LogWarning("Помилка UI: Не вдалося отримати PasswordBox з параметрів команди.");
                    ErrorMessage = "Сталася помилка. Не вдалося отримати пароль.";
                    return;
                }

                string password = passwordBox.Password;

                var query = new LoginUserQuery
                {
                    Email = Email,
                    Password = password,
                };

                LoginResultDto result = await _mediator.Send(query);

                if (result != null)
                {
                    _logger.LogInformation("Вхід успішний для користувача ID: {UserId}.", result.UserId);
                    ErrorMessage = string.Empty;
                    _userSession.CurrentUser = result;
                    _navigationService.ShowMainView();

                    var window = GetWindowFromParameter(parameter);
                    window?.Close();
                }
                else
                {
                    _logger.LogWarning("Спроба входу не вдалася: результат null (невірні дані?).");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час спроби входу користувача {Email}.", Email);
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GoToRegister(object parameter)
        {
            _logger.LogInformation("Перехід на сторінку реєстрації.");
            _navigationService.ShowRegister();

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