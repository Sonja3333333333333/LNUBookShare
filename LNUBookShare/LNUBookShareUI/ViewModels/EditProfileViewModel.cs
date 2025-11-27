//SRP 
//    Розділення LoadDataAsync: Створено окремі методи LoadUserProfileDataAsync() та LoadFacultiesAsync().	
//    Кожен метод тепер відповідає лише за одну задачу, підвищуючи читабельність та можливість тестування.

//DRY	
//    Централізація логіки UI: Створено приватні методи ShowErrorMessage() та CloseWindow(object window).	
//    Прибрано дублювання коду (повторювані MessageBox.Show та логіка закриття вікна) з методів Save, Cancel та завантаження даних.

//Meaningful Names	
//    Перейменовано логіку закриття у CloseWindow.



using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using MediatR;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Profile;
using LNUBookShareBLL.Features.Faculties;
using LNUBookShareUI.Common;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using LNUBookShareBLL.Features.Files;

namespace LNUBookShareUI.ViewModels
{
    public class EditProfileViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSession _userSession;

        private string _lastName = string.Empty;
        public string LastName
        {
            get => this._lastName;
            set => this.SetProperty(ref this._lastName, value);
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => this._firstName;
            set => this.SetProperty(ref this._firstName, value);
        }

        private string _profileImageUrl = string.Empty;
        public string ProfileImageUrl
        {
            get => this._profileImageUrl;
            set => this.SetProperty(ref this._profileImageUrl, value);
        }

        public ObservableCollection<FacultyDto> Faculties { get; } = new();

        private FacultyDto _selectedFaculty;
        public FacultyDto SelectedFaculty
        {
            get => this._selectedFaculty;
            set => this.SetProperty(ref this._selectedFaculty, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ICommand ChangePhotoCommand { get; }

        public EditProfileViewModel(IMediator mediator, IUserSession userSession)
        {
            this._mediator = mediator;
            this._userSession = userSession;
            this.SaveCommand = new RelayCommand<object>(async (w) => await this.Save(w));
            this.CancelCommand = new RelayCommand<object>(this.Cancel);
            this.ChangePhotoCommand = new RelayCommand(async () => await this.ChangePhoto());
        }

        private void ShowErrorMessage(string message, string title = "Помилка")
        {
            _ = MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CloseWindow(object window)
        {
            if (window is Window w)
            {
                w.Close();
            }
        }

        private async Task ChangePhoto()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    byte[] imageData = await File.ReadAllBytesAsync(filePath); 

                    var uploadCommand = new UploadImageCommand
                    {
                        FileName = Path.GetFileName(filePath),
                        ImageData = imageData
                    };

                    string newProfilePath = await this._mediator.Send(uploadCommand);
                    this.ProfileImageUrl = newProfilePath;
                }
                catch (Exception ex)
                {
                    this.ShowErrorMessage($"Не вдалося завантажити фото: {ex.Message}"); 
                }
            }
        }

        private async Task LoadUserProfileDataAsync()
        {
            var profileDto = await this._mediator.Send(new GetProfileForEditQuery { UserId = this._userSession.GetUserId() });

            this.LastName = profileDto.LastName;
            this.FirstName = profileDto.FirstName;
            this.ProfileImageUrl = profileDto.ProfileImageUrl;

            if (this.Faculties.Any())
            {
                this.SelectedFaculty = this.Faculties.FirstOrDefault(f => f.FacultyId == profileDto.FacultyId);
            }
        }

        private async Task LoadFacultiesAsync()
        {
            var facultyList = await this._mediator.Send(new GetAllFacultiesQuery());

            this.Faculties.Clear();
            foreach (var faculty in facultyList)
            {
                this.Faculties.Add(faculty);
            }
        }

        public async Task LoadDataAsync()
        {
            try
            {
                await this.LoadFacultiesAsync();
                await this.LoadUserProfileDataAsync();
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage($"Не вдалося завантажити дані профілю: {ex.Message}"); 
            }
        }

        private async Task Save(object window)
        {
            try
            {
                var profileDto = new ProfileEditDto
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                    FacultyId = this.SelectedFaculty?.FacultyId ?? 0, 
                    ProfileImageUrl = this.ProfileImageUrl
                };

                var command = new UpdateProfileCommand
                {
                    UserId = this._userSession.GetUserId(),
                    Dto = profileDto
                };
                _ = await this._mediator.Send(command);

                this.CloseWindow(window); 
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage($"Не вдалося зберегти профіль: {ex.Message}"); 
            }
        }

        private void Cancel(object window)
        {
            this.CloseWindow(window); 
        }
    }
}