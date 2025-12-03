using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;
using MediatR;
using System;
using System.Threading.Tasks;

namespace LNUBookShareConsole
{
    public class ConsoleController
    {
        private readonly IMediator _mediator;
        private readonly IDataSeeder _seeder; // Використовуємо інтерфейс
        private readonly IConsoleIO _console; // Використовуємо інтерфейс

        // Тепер приймаємо IConsoleIO через конструктор
        public ConsoleController(IMediator mediator, IDataSeeder seeder, IConsoleIO console)
        {
            this._mediator = mediator;
            this._seeder = seeder;
            this._console = console;
        }

        public async Task RunAsync()
        {
            _console.WriteLine("LNU Book Share Console. Введіть 'help' для списку команд.");
            bool isRunning = true;

            while (isRunning)
            {
                _console.SetColor(ConsoleColor.Green);
                _console.Write("> ");
                _console.ResetColor();

                string input = _console.ReadLine();

                // Якщо введено null (наприклад, кінець потоку в тестах), виходимо
                if (input == null) break;

                // Обробляємо команду
                // МИ ВИКЛИКАЄМО ОКРЕМИЙ ПУБЛІЧНИЙ МЕТОД, ЯКИЙ МОЖНА ТЕСТУВАТИ ОКРЕМО
                if (!await HandleCommandAsync(input))
                {
                    isRunning = false;
                }
            }
        }

        // Цей метод повертає false, якщо треба вийти (команда exit)
        public async Task<bool> HandleCommandAsync(string input)
        {
            switch (input?.ToLower().Trim())
            {
                case "help":
                    this.ShowHelp();
                    return true;

                case "seed":
                    _console.WriteLine("Введіть кількість записів (напр., 30):");
                    string countStr = _console.ReadLine();
                    if (int.TryParse(countStr, out int count))
                    {
                        await this._seeder.SeedDatabaseAsync(count);
                        _console.WriteLine("Базу даних успішно заповнено.");
                    }
                    else
                    {
                        _console.WriteLine("Некоректне число.");
                    }
                    return true;

                case "add-book":
                    await this.AddBookAsync();
                    return true;

                case "delete-book":
                    await this.DeleteBookAsync();
                    return true;

                case "exit":
                    return false; // Сигнал для зупинки циклу

                default:
                    _console.WriteLine("Невідома команда. Введіть 'help'.");
                    return true;
            }
        }

        private void ShowHelp()
        {
            _console.WriteLine("Доступні команди:");
            _console.WriteLine("  seed          - Запустити повну пере-генерацію БД");
            _console.WriteLine("  add-book      - Додати нову книгу (інтерактивно)");
            _console.WriteLine("  delete-book   - Видалити книгу за ID");
            _console.WriteLine("  exit          - Вийти з програми");
        }

        public async Task AddBookAsync() // Зробив public для зручного тестування
        {
            try
            {
                _console.Write("  Назва: ");
                string title = _console.ReadLine();

                _console.Write("  Автор: ");
                string author = _console.ReadLine();

                _console.Write("  ID Категорії: ");
                string catStr = _console.ReadLine();
                int categoryId = int.TryParse(catStr, out int id) ? id : 1; // Дефолтне значення якщо помилка

                var dto = new AddBookDto
                {
                    Title = title,
                    Author = author,
                    CategoryId = categoryId,
                };

                var command = new AddBookCommand
                {
                    Dto = dto,
                    OwnerUserId = 1, // Тимчасово хардкодимо ID юзера
                };

                int newBookId = await this._mediator.Send(command);

                _console.SetColor(ConsoleColor.Yellow);
                _console.WriteLine($"Успіх! Створено книгу з ID: {newBookId}");
                _console.ResetColor();
            }
            catch (Exception ex)
            {
                _console.SetColor(ConsoleColor.Red);
                _console.WriteLine($"Помилка: {ex.Message}");
                _console.ResetColor();
            }
        }

        public async Task DeleteBookAsync() // Зробив public
        {
            try
            {
                _console.Write("  Введіть ID книги для видалення: ");
                string idStr = _console.ReadLine();
                int bookId = int.Parse(idStr);

                var command = new DeleteBookCommand
                {
                    BookId = bookId,
                    CurrentUserId = 1,
                };

                await this._mediator.Send(command);

                _console.SetColor(ConsoleColor.Yellow);
                _console.WriteLine($"Успіх! Книгу {bookId} видалено.");
                _console.ResetColor();
            }
            catch (Exception ex)
            {
                _console.SetColor(ConsoleColor.Red);
                _console.WriteLine($"Помилка: {ex.Message}");
                _console.ResetColor();
            }
        }
    }
}