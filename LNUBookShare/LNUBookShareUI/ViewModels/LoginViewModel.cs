using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Auth;
using LNUBookShareUI.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LNUBookShareUI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;

        private string _email;
        private string _password;

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(IMediator mediator)
        {
            _mediator = mediator;
            LoginCommand = new RelayCommand(async () => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            try
            {
                // створюємо запит
                var query = new LoginUserQuery
                {
                    Email = this.Email,
                    Password = this.Password
                };

                // надсилаємо запит через MediatR
                var result = await _mediator.Send(query);

                // якщо вхід успішний
                if (result is LoginResultDto loginResult)
                {
                    MessageBox.Show(
                        $"Вітаємо, {loginResult.FirstName} {loginResult.LastName}!\nФакультет: {loginResult.FacultyName}",
                        "Вхід успішний",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    CloseLoginWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка входу", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseLoginWindow()
        {
            // закриває активне вікно
            Application.Current.Windows[0]?.Close();
        }
    }
}
