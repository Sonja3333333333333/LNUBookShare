using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Faculties;
using LNUBookShareBLL.Features.Files;
using LNUBookShareBLL.Features.Profile;

using LNUBookShareUI.Common;

using MediatR;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace LNUBookShareUI.ViewModels
{
    public class EditProfileViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSession _userSession;
        private readonly ILogger<EditProfileViewModel> _logger;

        private string _lastName = string.Empty;
        private string _firstName = string.Empty;
        private string _profileImageUrl = string.Empty;
        private FacultyDto _selectedFaculty;

        public EditProfileViewModel(IMediator mediator, IUserSession userSession, ILogger<EditProfileViewModel> logger)
        {
            _mediator = mediator;
            _userSession = userSession;
            _logger = logger;

            SaveCommand = new RelayCommand<object>(async (w) => await Save(w));
            CancelCommand = new RelayCommand<object>(Cancel);
            ChangePhotoCommand = new RelayCommand(async () => await ChangePhoto());

            _logger.LogInformation("EditProfileViewModel ініціалізовано.");
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string ProfileImageUrl
        {
            get => _profileImageUrl;
            set => SetProperty(ref _profileImageUrl, value);
        }

        public ObservableCollection<FacultyDto> Faculties { get; } = new ();

        public FacultyDto SelectedFaculty
        {
            get => _selectedFaculty;
            set => SetProperty(ref _selectedFaculty, value);
        }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ChangePhotoCommand { get; }

        public async Task LoadDataAsync()
        {
            int userId = _userSession.GetUserId();
            _logger.LogInformation("Користувач ID: {UserId} почав редагування профілю. Завантаження даних...", userId);

            try
            {
                var profileDto = await _mediator.Send(new GetProfileForEditQuery { UserId = userId });

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
                _logger.LogInformation("Дані профілю для користувача ID: {UserId} успішно завантажено.", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка завантаження даних профілю для користувача ID: {UserId}.", userId);
                _ = MessageBox.Show($"Не вдалося завантажити дані профілю: {ex.Message}");
            }
        }

        private async Task ChangePhoto()
        {
            int userId = _userSession.GetUserId();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    _logger.LogInformation("Користувач ID: {UserId} обрав нове фото: {FilePath}", userId, filePath);

                    byte[] imageData = File.ReadAllBytes(filePath);

                    var uploadCommand = new UploadImageCommand
                    {
                        FileName = Path.GetFileName(filePath),
                        ImageData = imageData,
                    };

                    string newProfilePath = await _mediator.Send(uploadCommand);

                    ProfileImageUrl = newProfilePath;
                    _logger.LogInformation("Нове фото профілю завантажено для користувача ID: {UserId}. Шлях: {Path}", userId, newProfilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Помилка завантаження фото для користувача ID: {UserId}.", userId);
                    _ = MessageBox.Show($"Не вдалося завантажити фото: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _logger.LogInformation("Користувач ID: {UserId} скасував вибір фото.", userId);
            }
        }

        private async Task Save(object window)
        {
            int userId = _userSession.GetUserId();
            _logger.LogInformation("Спроба збереження змін профілю для користувача ID: {UserId}.", userId);

            try
            {
                var profileDto = new ProfileEditDto
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    FacultyId = SelectedFaculty.FacultyId,
                    ProfileImageUrl = ProfileImageUrl,
                };

                var command = new UpdateProfileCommand
                {
                    UserId = userId,
                    Dto = profileDto,
                };
                _ = await _mediator.Send(command);

                _logger.LogInformation("Профіль користувача ID: {UserId} успішно оновлено.", userId);

                if (window is Window w)
                {
                    w.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка збереження профілю користувача ID: {UserId}.", userId);
                _ = MessageBox.Show($"Не вдалося зберегти профіль: {ex.Message}");
            }
        }

        private void Cancel(object window)
        {
            _logger.LogInformation("Користувач ID: {UserId} скасував редагування профілю.", _userSession.GetUserId());
            if (window is Window w)
            {
                w.Close();
            }
        }
    }
}