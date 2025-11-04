using LNUBookShareUI.ViewModels;
using LNUBookShareUI.Views;
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
        public void ShowFavorites()
        {
            var favoritesView = _serviceProvider.GetService<FavoritesView>();

            var favoritesViewModel = _serviceProvider.GetService<FavoritesViewModel>();

            favoritesView.DataContext = favoritesViewModel;

            favoritesView.Show();
        }
    }
}
