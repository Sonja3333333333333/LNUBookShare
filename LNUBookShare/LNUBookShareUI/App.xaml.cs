// У вашому головному проєкті, наприклад, App.xaml.cs (для WPF)
using LNUBookShareDAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        ServiceCollection services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 1. Створюємо конфігурацію, щоб прочитати appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. Отримуємо рядок підключення
        string connectionString = configuration.GetConnectionString("DefaultConnection");

        // 3. Реєструємо наш DbContext. 
        // Це головна команда!
        // Вона каже: "Коли хтось попросить LNUBookShareDbContext, 
        // створи його і використай цей рядок підключення PostgreSQL."
        services.AddDbContext<LNUBookShareDbContext>(options =>
            options.UseNpgsql(connectionString)
        );

        // Сюди ж ви будете додавати ваші вікна, 
        // сервіси логіки тощо
        // services.AddSingleton<MainWindow>(); 
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        // Цей код (або схожий) потрібен, щоб 
        // відкрити ваше головне вікно,
        // але він залежить від вашої архітектури (MVVM чи іншої)

        // var mainWindow = ServiceProvider.GetService<MainWindow>();
        // mainWindow.Show();
    }
}