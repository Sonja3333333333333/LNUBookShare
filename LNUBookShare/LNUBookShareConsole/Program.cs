using LNUBookShareBLL.Features.Books;

using LNUBookShareConsole;

using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
Console.OutputEncoding = System.Text.Encoding.UTF8;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string connectionString = context.Configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<LNUBookShareDbContext>(options =>
            options.UseNpgsql(connectionString)

            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

        services.AddMediatR(typeof(AddBookCommand).Assembly);

        services.AddTransient<DataSeeder>();
        services.AddTransient<ConsoleController>();
    })
    .Build();

await RunConsoleApp(host.Services);

Console.WriteLine("Роботу завершено. Натисніть Enter для виходу.");
Console.ReadLine();

static async Task RunConsoleApp(IServiceProvider services)
{
    using (var scope = services.CreateScope())
    {
        var controller = scope.ServiceProvider.GetRequiredService<ConsoleController>();

        await controller.RunAsync();
    }
}