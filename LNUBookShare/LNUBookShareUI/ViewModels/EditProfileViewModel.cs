using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Linq; 
using MediatR;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Profile;
using LNUBookShareBLL.Features.Faculties; 

namespace LNUBookShareUI.ViewModels
{
    public partial class EditProfileViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private int _currentUserId = 1; 

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _profileImageUrl = string.Empty; 

        public ObservableCollection<FacultyDto> Faculties { get; } = new();

        [ObservableProperty]
        private FacultyDto _selectedFaculty;

        public EditProfileViewModel(IMediator mediator)
        {
            _mediator = mediator;
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

        [RelayCommand]
        private async Task Save(object window)
        {
            try
            {
                var profileDto = new ProfileEditDto
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                    FacultyId = this.SelectedFaculty.FacultyId
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

        [RelayCommand]
        private void Cancel(object window)
        {
            if (window is Window w) { w.Close(); }
        }
    }
}