using LNUBookShareDAL;
using LNUBookShareBLL.Features.Books;
using LNUBookShareUI.ViewModels;
using LNUBookShareUI.Views;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using LNUBookShareDAL.Models;

namespace LNUBookShareUI
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
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
            // (Сюди додаси LoginViewModel, RegisterViewModel...)

            services.AddTransient<LoginViewModel>();
            services.AddTransient<LoginView>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<RegisterView>();

            // 4. Views
            services.AddTransient<MainView>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainView = _serviceProvider.GetService<MainView>();
            var mainViewModel = _serviceProvider.GetService<MainViewModel>();

            mainView.DataContext = mainViewModel;
            mainView.Show();
        }
    }
}