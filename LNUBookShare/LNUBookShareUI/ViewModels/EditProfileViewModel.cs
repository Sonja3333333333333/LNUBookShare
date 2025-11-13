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
    // 1. Змінено ObservableObject на ViewModelBase
    public class EditProfileViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private int _currentUserId = 1;

        // 2. Змінено [ObservableProperty] на повні властивості
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

        public EditProfileViewModel(IMediator mediator)
        {
            this._mediator = mediator;

            // 4. Ініціалізовано команди
            this.SaveCommand = new RelayCommand<object>(async (w) => await this.Save(w));
            this.CancelCommand = new RelayCommand<object>(this.Cancel);
            this.ChangePhotoCommand = new RelayCommand(async () => await this.ChangePhoto());
        }

        private async Task ChangePhoto()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    byte[] imageData = File.ReadAllBytes(filePath);

                    
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
                    _ = MessageBox.Show($"Не вдалося завантажити фото: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public async Task LoadDataAsync()
            {
                try
                {
                    var profileDto = await this._mediator.Send(new GetProfileForEditQuery { UserId = _currentUserId });

                this.LastName = profileDto.LastName;
                this.FirstName = profileDto.FirstName;
                this.ProfileImageUrl = profileDto.ProfileImageUrl;

                    var facultyList = await this._mediator.Send(new GetAllFacultiesQuery());
                this.Faculties.Clear();
                    foreach (var faculty in facultyList)
                    {
                    this.Faculties.Add(faculty);
                    }

                this.SelectedFaculty = this.Faculties.FirstOrDefault(f => f.FacultyId == profileDto.FacultyId);
                }
                catch (Exception ex)
                {
                _ = MessageBox.Show($"Не вдалося завантажити дані профілю: {ex.Message}");
                }
            }

        // 5. Видалено [RelayCommand]
        private async Task Save(object window)
        {
            try
            {
                var profileDto = new ProfileEditDto
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                    FacultyId = this.SelectedFaculty.FacultyId,

                    ProfileImageUrl = this.ProfileImageUrl
                };

                var command = new UpdateProfileCommand
                {
                    UserId = _currentUserId,
                    Dto = profileDto
                };
                _ = await this._mediator.Send(command);

                if (window is Window w) { w.Close(); }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Не вдалося зберегти профіль: {ex.Message}");
            }
        }

        // 6. Видалено [RelayCommand]
        private void Cancel(object window)
        {
            if (window is Window w) { w.Close(); }
        }
    }
}