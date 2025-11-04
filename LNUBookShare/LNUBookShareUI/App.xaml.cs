using LNUBookShareBLL.Features.Books;
using LNUBookShareUI.ViewModels;
using LNUBookShareUI.Views;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using LNUBookShareDAL.Models;
using System;
using LNUBookShareUI.Common;

namespace LNUBookShareUI
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {

            string connectionString = "Host=localhost;Database=LNUBookShare;Username=postgres;Password=135798852";
            services.AddDbContext<LNUBookShareDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            // 2. BLL
            services.AddMediatR(typeof(GetBooksQuery).Assembly);

            // 3. ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainView>();

            services.AddTransient<ProfileViewModel>();
            services.AddTransient<ProfileView>();

            services.AddTransient<BookDetailsView>();
            services.AddTransient<BookDetailsViewModel>();

            services.AddTransient<FavoritesView>();
            services.AddTransient<FavoritesViewModel>();

            services.AddSingleton<INavigationService, NavigationService>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainView = _serviceProvider.GetService<MainView>();
            var mainViewModel = _serviceProvider.GetService<MainViewModel>();
            mainView.DataContext = mainViewModel;
            mainView.Show();


            //var profileView = _serviceProvider.GetService<ProfileView>();
            //var profileViewModel = _serviceProvider.GetService<ProfileViewModel>();
            //profileView.DataContext = profileViewModel;
            //profileView.Show();
        }
    }
}