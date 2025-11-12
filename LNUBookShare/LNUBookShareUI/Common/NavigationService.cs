using LNUBookShareUI.ViewModels;
using LNUBookShareUI.Views;
using MediatR;
using Microsoft.Extensions.DependencyInjection; 
using System;
using System.Windows;

namespace LNUBookShareUI.Common
{
    
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async void ShowBookDetails(int bookId)
        {
            try
            {

                var view = _serviceProvider.GetService<BookDetailsView>();

                var viewModel = new BookDetailsViewModel(
                    _serviceProvider.GetService<IMediator>(),
                    this
                );

                // 3. З'єднуємо їх
                view.DataContext = viewModel;

                // 4. Асинхронно завантажуємо дані
                await viewModel.LoadBookDetailsAsync(bookId);

                // 5. Показуємо вікно
                view.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося відкрити деталі книги: {ex.Message}");
            }
        }

        public void ShowProfile()
        {
            // 1. Просимо у "фабрики" нове вікно
            var profileView = _serviceProvider.GetService<ProfileView>();

            // 2. Просимо у "фабрики" нову ViewModel
            var profileViewModel = _serviceProvider.GetService<ProfileViewModel>();

            // 3. З'єднуємо їх
            profileView.DataContext = profileViewModel;

            // 4. Показуємо вікно
            profileView.Show();

        }
        public void ShowViewProfile(int id)
        {
            var profileView = _serviceProvider.GetService<ProfileView>();


            var viewOtherProfileViewModel = new ViewOtherProfileViewModel(
                _serviceProvider.GetService<IMediator>(),
                this,
                id
            );

            profileView.DataContext = viewOtherProfileViewModel;
            profileView.Show();
        }
        public void ShowFavorites()
        {
            var favoritesView = _serviceProvider.GetService<FavoritesView>();

            var favoritesViewModel = _serviceProvider.GetService<FavoritesViewModel>();

            favoritesView.DataContext = favoritesViewModel;

            favoritesView.Show();
        }

        public void ShowMainView()
        {
            var mainView = _serviceProvider.GetService<MainView>();
            mainView.DataContext = _serviceProvider.GetService<MainViewModel>();
            mainView.Show();
        }

        public void ShowLogin()
        {
            var loginView = _serviceProvider.GetService<LoginView>();
            loginView.DataContext = _serviceProvider.GetService<LoginViewModel>();
            loginView.Show();
        }

        public void ShowRegister()
        {
            var registerView = _serviceProvider.GetService<RegisterView>();
            registerView.DataContext = _serviceProvider.GetService<RegisterViewModel>();
            registerView.Show();
        }
        public async void ShowEditProfile()
        {
            var editProfileView = _serviceProvider.GetService<EditProfileView>();
            var editProfileViewModel = _serviceProvider.GetService<EditProfileViewModel>();
            await editProfileViewModel.LoadDataAsync();
            editProfileView.DataContext = editProfileViewModel;
            editProfileView.ShowDialog();
        }

    }
}
