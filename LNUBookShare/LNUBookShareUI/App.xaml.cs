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

           
            // string connectionString = "Host=localhost;Database=LNUBookShare;Username=postgres;Password=135798852";

            // Новий рядок (хмарний Neon)
            // Додано "Trust Server Certificate=true" для сумісності з Npgsql v7
            string connectionString = "Host=ep-wispy-hat-adm0eu4d-pooler.c-2.us-east-1.aws.neon.tech;" +
                                      "Database=neondb;" +
                                      "Username=neondb_owner;" +
                                      "Password=npg_GqkRolz4rhy6;" +
                                      "SSL Mode=Require;" +
                                      "Trust Server Certificate=true"; 
          

            services.AddDbContext<LNUBookShareDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            // 2. BLL
            services.AddMediatR(typeof(GetBooksQuery).Assembly);

            // 3. ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<RegisterViewModel>();

            services.AddTransient<LoginView>();
            services.AddTransient<RegisterView>();
            services.AddTransient<MainView>();
            services.AddTransient<ProfileView>();

            services.AddTransient<BookDetailsView>();
            services.AddTransient<BookDetailsViewModel>();

            services.AddTransient<FavoritesView>();
            services.AddTransient<FavoritesViewModel>();

            services.AddSingleton<INavigationService, NavigationService>();

            services.AddTransient<Func<int, ViewOtherProfileViewModel>>(provider => userId =>
                new ViewOtherProfileViewModel(
                    provider.GetService<IMediator>(),
                    provider.GetService<INavigationService>(), // <-- ДОДАНО СЕРВІС НАВІГАЦІЇ
                    userId
                ));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Щоб показати головне вікно розкоментуй мене
            var mainView = _serviceProvider.GetService<MainView>();
            var mainViewModel = _serviceProvider.GetService<MainViewModel>();
            mainView.DataContext = mainViewModel;
            mainView.Show();

            //Щоб показати автентифікацію розкоментуй мене
            //var loginView = _serviceProvider.GetService<LoginView>();
            //loginView.DataContext = _serviceProvider.GetService<LoginViewModel>();
            //loginView.Show();

            //Щоб показати вікно Профіль розкоментуй мене
            //var profileView = _serviceProvider.GetService<ProfileView>();
            //var profileViewModel = _serviceProvider.GetService<ProfileViewModel>();
            //profileView.DataContext = profileViewModel;
            //profileView.Show();
        }
    }
}