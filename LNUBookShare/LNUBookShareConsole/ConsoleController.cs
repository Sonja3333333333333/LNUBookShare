using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;

using MediatR;


namespace LNUBookShareConsole
{
    public class ConsoleController
    {
        private readonly IMediator _mediator;
        private readonly DataSeeder _seeder;


        public ConsoleController(IMediator mediator, DataSeeder seeder)
        {
            this._mediator = mediator;
            this._seeder = seeder;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("LNU Book Share Console. Введіть 'help' для списку команд.");
            bool isRunning = true;

            while (isRunning)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("> ");
                Console.ResetColor();
                string input = Console.ReadLine();

                // Обробляємо команду
                switch (input?.ToLower().Trim())
                {
                    case "help":
                        this.ShowHelp();
                        break;

                    case "seed":
                        Console.WriteLine("Введіть кількість записів (напр., 30):");
                        int count = int.Parse(Console.ReadLine());
                        await this._seeder.SeedDatabaseAsync(count);
                        Console.WriteLine("Базу даних успішно заповнено.");
                        break;

                    case "add-book":
                        await this.AddBookAsync();
                        break;

                    case "delete-book":
                        await this.DeleteBookAsync();
                        break;

                    case "exit":
                        isRunning = false;
                        break;

                    default:
                        Console.WriteLine("Невідома команда. Введіть 'help'.");
                        break;
                }
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("Доступні команди:");
            Console.WriteLine("  seed         - Запустити повну пере-генерацію БД");
            Console.WriteLine("  add-book     - Додати нову книгу (інтерактивно)");
            Console.WriteLine("  delete-book  - Видалити книгу за ID");
            Console.WriteLine("  exit         - Вийти з програми");
        }


        private async Task AddBookAsync()
        {
            try
            {
                Console.Write("  Назва: ");
                string title = Console.ReadLine();
                Console.Write("  Автор: ");
                string author = Console.ReadLine();
                Console.Write("  ID Категорії: ");
                int categoryId = int.Parse(Console.ReadLine());



                var dto = new AddBookDto
                {
                    Title = title,
                    Author = author,
                    CategoryId = categoryId
                };

                var command = new AddBookCommand
                {
                    Dto = dto,
                    OwnerUserId = 1
                };


                int newBookId = await this._mediator.Send(command);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Успіх! Створено книгу з ID: {newBookId}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка: {ex.Message}");
                Console.ResetColor();
            }
        }

        private async Task DeleteBookAsync()
        {
            try
            {
                Console.Write("  Введіть ID книги для видалення: ");
                int bookId = int.Parse(Console.ReadLine());

                var command = new DeleteBookCommand
                {
                    BookId = bookId,
                    CurrentUserId = 1
                };

                await this._mediator.Send(command);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Успіх! Книгу {bookId} видалено.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}