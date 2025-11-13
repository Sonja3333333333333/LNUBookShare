using LNUBookShareUI.ViewModels;
using LNUBookShareUI.Views;
using MediatR;
using Microsoft.Extensions.DependencyInjection; 
using System;
using System.Threading.Tasks;
using System.Windows;

namespace LNUBookShareUI.Common
{
    
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public async Task ShowEditBookAsync(int bookId)
        {
            var view = this._serviceProvider.GetService<EditBookView>();
            var viewModel = this._serviceProvider.GetService<EditBookViewModel>();

            view.DataContext = viewModel;

            // Завантажуємо категорії ТА дані книги
            await viewModel.LoadDataAsync(bookId);

            _ = view.ShowDialog();
        }

        public async Task ShowAddBookAsync()
        {
            var view = this._serviceProvider.GetService<AddBookView>();
            var viewModel = this._serviceProvider.GetService<AddBookViewModel>();

            view.DataContext = viewModel;

            // Завантажуємо категорії ДО показу вікна
            await viewModel.LoadDataAsync();

            // Показуємо як діалог
            _ = view.ShowDialog();
        }

        public async void ShowBookDetails(int bookId)
        {
            try
            {

                var view = this._serviceProvider.GetService<BookDetailsView>();

                var viewModel = new BookDetailsViewModel(
                    this._serviceProvider.GetService<IMediator>(),
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
                _ = MessageBox.Show($"Не вдалося відкрити деталі книги: {ex.Message}");
            }
        }

        public void ShowProfile()
        {
            // 1. Просимо у "фабрики" нове вікно
            var profileView = this._serviceProvider.GetService<ProfileView>();

            // 2. Просимо у "фабрики" нову ViewModel
            var profileViewModel = this._serviceProvider.GetService<ProfileViewModel>();

            // 3. З'єднуємо їх
            profileView.DataContext = profileViewModel;

            // 4. Показуємо вікно
            profileView.Show();

        }
        public void ShowViewProfile(int id)
        {
            var profileView = this._serviceProvider.GetService<ProfileView>();


            var viewOtherProfileViewModel = new ViewOtherProfileViewModel(
                this._serviceProvider.GetService<IMediator>(),
                this,
                id
            );

            profileView.DataContext = viewOtherProfileViewModel;
            profileView.Show();
        }
        public void ShowFavorites()
        {
            var favoritesView = this._serviceProvider.GetService<FavoritesView>();

            var favoritesViewModel = this._serviceProvider.GetService<FavoritesViewModel>();

            favoritesView.DataContext = favoritesViewModel;

            favoritesView.Show();
        }

        public void ShowMainView()
        {
            var mainView = this._serviceProvider.GetService<MainView>();
            mainView.DataContext = this._serviceProvider.GetService<MainViewModel>();
            mainView.Show();
        }

        public void ShowLogin()
        {
            var loginView = this._serviceProvider.GetService<LoginView>();
            loginView.DataContext = this._serviceProvider.GetService<LoginViewModel>();
            loginView.Show();
        }

        public void ShowRegister()
        {
            var registerView = this._serviceProvider.GetService<RegisterView>();
            registerView.DataContext = this._serviceProvider.GetService<RegisterViewModel>();
            registerView.Show();
        }
        public async Task ShowEditProfile()
        {
            var editProfileView = this._serviceProvider.GetService<EditProfileView>();
            var editProfileViewModel = this._serviceProvider.GetService<EditProfileViewModel>();

            await editProfileViewModel.LoadDataAsync();

            editProfileView.DataContext = editProfileViewModel;

            _ = editProfileView.ShowDialog();
        }

    }
}
