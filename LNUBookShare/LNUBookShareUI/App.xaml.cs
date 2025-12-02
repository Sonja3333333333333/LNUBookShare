using LNUBookShareBLL;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Files;
using LNUBookShareDAL.Models;
using LNUBookShareUI.Common;
using LNUBookShareUI.ViewModels;
using LNUBookShareUI.Views;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace LNUBookShareUI
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var services = new ServiceCollection();
            this.ConfigureServices(services);
            this._serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //var editProfileView = _serviceProvider.GetService<EditProfileView>();
            //var editProfileViewModel = _serviceProvider.GetService<EditProfileViewModel>();
            //editProfileView.DataContext = editProfileViewModel;
            //editProfileView.Show();

            //Щоб показати головне вікно розкоментуй мене
            //var mainView = this._serviceProvider.GetService<MainView>();
            //var mainViewModel = this._serviceProvider.GetService<MainViewModel>();
            //mainView.DataContext = mainViewModel;
            //mainView.Show();

            //Щоб показати автентифікацію розкоментуй мене
            var loginView = _serviceProvider.GetService<LoginView>();
            loginView.DataContext = _serviceProvider.GetService<LoginViewModel>();
            loginView.Show();

            //Щоб показати вікно Профіль розкоментуй мене
            //var profileView = _serviceProvider.GetService<ProfileView>();
            //var profileViewModel = _serviceProvider.GetService<ProfileViewModel>();
            //profileView.DataContext = profileViewModel;
            //profileView.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // string connectionString = "Host=localhost;Database=LNUBookShare;Username=postgres;Password=135798852";

            string connectionString = "Host=ep-wispy-hat-adm0eu4d-pooler.c-2.us-east-1.aws.neon.tech;" +
                                      "Database=neondb;" +
                                      "Username=neondb_owner;" +
                                      "Password=npg_GqkRolz4rhy6;" +
                                      "SSL Mode=Require;" +
                                      "Trust Server Certificate=true";


                    _ = services.AddDbContext<LNUBookShareDbContext>(options =>
              options.UseNpgsql(connectionString),
              ServiceLifetime.Transient
          );

            _ = services.AddMediatR(typeof(GetBooksQuery).Assembly);
            _ = services.AddMediatR(typeof(UploadImageCommand).Assembly);


            // _ = services.AddTransient<EmailService>();
            _ = services.AddTransient<MainViewModel>();
            _ = services.AddTransient<LoginViewModel>();
            _ = services.AddTransient<ProfileViewModel>();
            _ = services.AddTransient<RegisterViewModel>();

            _ = services.AddTransient<LoginView>();
            _ = services.AddTransient<RegisterView>();
            _ = services.AddTransient<MainView>();
            _ = services.AddTransient<ProfileView>();

            _ = services.AddTransient<BookDetailsView>();
            _ = services.AddTransient<BookDetailsViewModel>();

            _ = services.AddTransient<FavoritesView>();
            _ = services.AddTransient<FavoritesViewModel>();

            _ = services.AddTransient<EditProfileView>();
            _ = services.AddTransient<EditProfileViewModel>();

            _ = services.AddTransient<AddBookView>();
            _ = services.AddTransient<AddBookViewModel>();

            _ = services.AddTransient<EditBookView>();
            _ = services.AddTransient<EditBookViewModel>();

            _ = services.AddSingleton<INavigationService, NavigationService>();

            _ = services.AddSingleton<IUserSession, UserSession>();

            _ = services.AddTransient<Func<int, ViewOtherProfileViewModel>>(provider => userId =>
                new ViewOtherProfileViewModel(
                    provider.GetService<IMediator>(),
                    provider.GetService<INavigationService>(),
                    userId
                ));

            services.AddTransient<EmailService>();
            services.AddTransient<IEmailService, EmailService>();
        }
    }
}