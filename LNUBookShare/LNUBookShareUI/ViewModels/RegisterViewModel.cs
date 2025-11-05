using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Auth;
using LNUBookShareBLL.Features.Faculties;
using LNUBookShareUI.Common;
using LNUBookShareUI.Views;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace LNUBookShareUI.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;

        // --- Властивості  ---
        private string _firstName;
        private string _lastName;
        private string _email;
        private string _password;
        private FacultyDto _selectedFaculty;

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public FacultyDto SelectedFaculty
        {
            get => _selectedFaculty;
            set { _selectedFaculty = value; OnPropertyChanged(); }
        }

        // --- Колекція факультетів для ComboBox ---
        public ObservableCollection<FacultyDto> Faculties { get; } = new();

        // --- Команди ---
        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        // --- Конструктор ---
        public RegisterViewModel(IMediator mediator)
        {
            _mediator = mediator;

            RegisterCommand = new RelayCommand(async () => await RegisterAsync());
            GoToLoginCommand = new RelayCommand(GoToLogin);

            _ = LoadFacultiesAsync(); // одразу завантажуємо факультети
        }

        // --- Метод реєстрації ---
        private async Task RegisterAsync()
        {
            try
            {
                // Перевірка полів (можна зробити більш строгі)
                if (string.IsNullOrWhiteSpace(FirstName) ||
                    string.IsNullOrWhiteSpace(LastName) ||
                    string.IsNullOrWhiteSpace(Email) ||
                    string.IsNullOrWhiteSpace(Password) ||
                    SelectedFaculty == null)
                {
                    MessageBox.Show("Будь ласка, заповніть усі поля.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Створюємо команду
                var command = new RegisterUserCommand
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                    Email = this.Email,
                    Password = this.Password,
                    FacultyId = this.SelectedFaculty.FacultyId
                };

                // Відправляємо через MediatR
                await _mediator.Send(command);

                // Якщо успішно — повідомлення і перехід до логіну
                MessageBox.Show("Перевірте пошту для підтвердження реєстрації.", "Реєстрація успішна",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                GoToLogin();
            }
            catch (Exception ex)
            {
                // Якщо помилка, показуємо повідомлення користувачу
                MessageBox.Show(ex.Message, "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Завантаження факультетів у ComboBox ---
        private async Task LoadFacultiesAsync()
        {
            try
            {
                var faculties = await _mediator.Send(new GetAllFacultiesQuery());

                Faculties.Clear();
                foreach (var f in faculties)
                    Faculties.Add(f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не вдалося завантажити список факультетів.\n" + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Перехід назад до LoginView ---
        private void GoToLogin()
        {
            // Закриваємо поточне вікно
            Application.Current.Windows[0]?.Close();

            // Відкриваємо нове вікно входу
            var loginView = new LoginView();
            loginView.Show();
        }

    }
}
