using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Auth;

using LNUBookShareUI.Common;

using MediatR;

namespace LNUBookShareUI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;

        private string _email;
        private string _errorMessage;

        public LoginViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession)
        {
            _mediator = mediator;
            _navigationService = navigationService;
            _userSession = userSession;

            LoginCommand = new RelayCommand<object>(async (param) => await LoginAsync(param));
            GoToRegisterCommand = new RelayCommand<object>(GoToRegister);
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
            
            try
            {
                if (parameter is not PasswordBox passwordBox)
                {
                    ErrorMessage = "Сталася помилка. Не вдалося отримати пароль.";
                    return;
                }

                string password = passwordBox.Password;

                var query = new LoginUserQuery
                {
                    Email = Email,
                    Password = password,
                };

                //await Task.Delay(3000);
                LoginResultDto result = await _mediator.Send(query);

                if (result != null)
                {
                    ErrorMessage = string.Empty;
                    _userSession.CurrentUser = result;
                    _navigationService.ShowMainView();

                    var window = GetWindowFromParameter(parameter);
                    window?.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false; 
            }
        }

        private void GoToRegister(object parameter)
        {
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