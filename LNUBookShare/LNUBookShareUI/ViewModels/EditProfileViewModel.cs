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
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _profileImageUrl = string.Empty;
        public string ProfileImageUrl
        {
            get => _profileImageUrl;
            set => SetProperty(ref _profileImageUrl, value);
        }

        public ObservableCollection<FacultyDto> Faculties { get; } = new();

        private FacultyDto _selectedFaculty;
        public FacultyDto SelectedFaculty
        {
            get => _selectedFaculty;
            set => SetProperty(ref _selectedFaculty, value);
        }

        
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ICommand ChangePhotoCommand { get; }

        public EditProfileViewModel(IMediator mediator)
        {
            _mediator = mediator;

            // 4. Ініціалізовано команди
            SaveCommand = new RelayCommand<object>(async (w) => await Save(w));
            CancelCommand = new RelayCommand<object>(Cancel);
            ChangePhotoCommand = new RelayCommand(async () => await ChangePhoto());
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

                  
                    string newProfilePath = await _mediator.Send(uploadCommand);

                    
                    ProfileImageUrl = newProfilePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося завантажити фото: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public async Task LoadDataAsync()
            {
                try
                {
                    var profileDto = await _mediator.Send(new GetProfileForEditQuery { UserId = _currentUserId });

                    LastName = profileDto.LastName;
                    FirstName = profileDto.FirstName;
                    ProfileImageUrl = profileDto.ProfileImageUrl;

                    var facultyList = await _mediator.Send(new GetAllFacultiesQuery());
                    Faculties.Clear();
                    foreach (var faculty in facultyList)
                    {
                        Faculties.Add(faculty);
                    }

                    SelectedFaculty = Faculties.FirstOrDefault(f => f.FacultyId == profileDto.FacultyId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося завантажити дані профілю: {ex.Message}");
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
                await _mediator.Send(command);

                if (window is Window w) { w.Close(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося зберегти профіль: {ex.Message}");
            }
        }

        // 6. Видалено [RelayCommand]
        private void Cancel(object window)
        {
            if (window is Window w) { w.Close(); }
        }
    }
}