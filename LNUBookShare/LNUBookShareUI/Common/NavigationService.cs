using LNUBookShareUI.ViewModels;
using LNUBookShareUI.Views;
using MediatR;
using Microsoft.Extensions.DependencyInjection; 
using System;

namespace LNUBookShareUI.Common
{
    
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
                      

            var viewOtherProfileViewModel = new ViewOtherProfileViewModel(_serviceProvider.GetService<IMediator>(), id);

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

    }
}
