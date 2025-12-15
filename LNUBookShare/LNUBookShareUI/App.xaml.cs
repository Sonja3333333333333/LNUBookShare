using System;
using System.Windows;
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
using Microsoft.Extensions.Logging;
using Serilog;

namespace LNUBookShareUI
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.File("logs/app_log.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();

            Log.Information("=== Додаток запускається ===");

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var services = new ServiceCollection();
            this.ConfigureServices(services);
            this._serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                Log.Information("Відкриття логіну");

                var loginView = _serviceProvider.GetService<LoginView>();
                loginView.DataContext = _serviceProvider.GetService<LoginViewModel>();
                loginView.Show();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критична помилка при запуску додатку!");
                MessageBox.Show("Критична помилка. Перевірте логи.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders(); 
                loggingBuilder.AddSerilog(dispose: true); 
            });

            // string connectionString = "Host=localhost;Database=LNUBookShare;Username=postgres;Password=135798852";
            string connectionString = "Host=ep-wispy-hat-adm0eu4d-pooler.c-2.us-east-1.aws.neon.tech;" +
                                      "Database=neondb;" +
                                      "Username=neondb_owner;" +
                                      "Password=npg_GqkRolz4rhy6;" +
                                      "SSL Mode=Require;" +
                                      "Trust Server Certificate=true";

            _ = services.AddDbContext<LNUBookShareDbContext>(
                options =>
      options.UseNpgsql(connectionString),
                ServiceLifetime.Transient);

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
                    userId));

            services.AddTransient<EmailService>();
            services.AddTransient<IEmailService, EmailService>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("=== Додаток завершує роботу ===");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}