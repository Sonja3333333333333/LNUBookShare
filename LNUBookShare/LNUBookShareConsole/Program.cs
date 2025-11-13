using LNUBookShareDAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LNUBookShareConsole;
using LNUBookShareBLL.Features.Books; // <-- Потрібно для MediatR
using MediatR;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
Console.OutputEncoding = System.Text.Encoding.UTF8;

// 1. Налаштовуємо Host та DI
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string connectionString = context.Configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<LNUBookShareDbContext>(options =>
            options.UseNpgsql(connectionString)
            // (Закоментуйте LogTo, якщо не хочете бачити SQL-запити)
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
        );

        // Реєструємо BLL (MediatR)
        services.AddMediatR(typeof(AddBookCommand).Assembly);

        // Реєструємо наші класи з ConsoleApp
        services.AddTransient<DataSeeder>();
        services.AddTransient<ConsoleController>();
    })
    .Build();

// 2. Викликаємо головну логіку
await RunConsoleApp(host.Services);

Console.WriteLine("Роботу завершено. Натисніть Enter для виходу.");
Console.ReadLine();


// ========== ГОЛОВНА ЛОГІКА ЗАПУСКУ ==========
static async Task RunConsoleApp(IServiceProvider services)
{
    // Отримуємо контролер з DI
    using (var scope = services.CreateScope())
    {
        var controller = scope.ServiceProvider.GetRequiredService<ConsoleController>();

        // Запускаємо головний цикл програми
        await controller.RunAsync();
    }
}