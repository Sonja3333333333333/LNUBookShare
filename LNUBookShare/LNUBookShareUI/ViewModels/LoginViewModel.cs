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

        // --- Конструктор ---
        public LoginViewModel(IMediator mediator, INavigationService navigationService, IUserSession userSession)
        {
            this._mediator = mediator;
            this._navigationService = navigationService;
            this._userSession = userSession; 

            this.LoginCommand = new RelayCommand<object>(async (param) => await this.LoginAsync(param));
            this.GoToRegisterCommand = new RelayCommand<object>(this.GoToRegister);
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
           
                var query = new LoginUserQuery
                {
                    Email = this.Email,
                    Password = password
                };

                LoginResultDto result = await this._mediator.Send(query);

                if (result != null)
                {
                   
                    this.ErrorMessage = ""; 
                    this._userSession.CurrentUser = result;
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

        private void GoToRegister(object parameter)
        {
            this._navigationService.ShowRegister();

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