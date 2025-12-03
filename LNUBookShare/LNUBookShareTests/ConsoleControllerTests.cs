using Xunit;
using Moq;
using MediatR;
using LNUBookShareConsole;
using LNUBookShareBLL.Features.Books;
using System.Threading;
using System.Threading.Tasks;
using System;
using LNUBookShareBLL.DTOs; // Додайте, якщо AddBookDto в іншому namespace

namespace LNUBookShare.Tests
{
    public class ConsoleControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IDataSeeder> _seederMock;
        private readonly Mock<IConsoleIO> _consoleMock;
        private readonly ConsoleController _controller;

        public ConsoleControllerTests()
        {
            // 1. Створюємо "фейкові" залежності (Mock Objects)
            // Вони імітують поведінку реальних об'єктів, але ми їх повністю контролюємо.
            _mediatorMock = new Mock<IMediator>();
            _seederMock = new Mock<IDataSeeder>();
            _consoleMock = new Mock<IConsoleIO>();

            // 2. Створюємо реальний контролер, але передаємо йому наші "фейки"
            _controller = new ConsoleController(_mediatorMock.Object, _seederMock.Object, _consoleMock.Object);
        }

        [Fact]
        public async Task HandleCommandAsync_ShouldCallSeeder_WhenCommandIsSeed()
        {
            // --- ARRANGE (Підготовка) ---
            // Коли контролер спитає "скільки записів?", ми кажемо йому "50"
            // Налаштовуємо _consoleMock, щоб на виклик ReadLine() він повернув "50"
            _consoleMock.Setup(c => c.ReadLine()).Returns("50");

            // --- ACT (Дія) ---
            // Викликаємо метод, який ми тестуємо, з командою "seed"
            await _controller.HandleCommandAsync("seed");

            // --- ASSERT (Перевірка) ---
            // Перевіряємо, що метод SeedDatabaseAsync у сідера був викликаний РІВНО 1 РАЗ із параметром 50
            _seederMock.Verify(s => s.SeedDatabaseAsync(50), Times.Once);

            // Перевіряємо, що контролер написав у консоль повідомлення про успіх
            _consoleMock.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("успішно"))), Times.Once);
        }

        [Fact]
        public async Task AddBookAsync_ShouldSendMediatorCommand_WithCorrectData()
        {
            // --- ARRANGE ---
            // Налаштовуємо послідовність відповідей користувача для ReadLine():
            // 1-й виклик (Назва): "Кобзар"
            // 2-й виклик (Автор): "Т. Шевченко"
            // 3-й виклик (Категорія): "5"
            _consoleMock.SetupSequence(c => c.ReadLine())
                .Returns("Кобзар")
                .Returns("Т. Шевченко")
                .Returns("5");

            // Налаштовуємо Mediator, щоб він повертав ID = 100 при успішному виконанні
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<AddBookCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(100);

            // --- ACT ---
            // Викликаємо метод додавання книги
            await _controller.AddBookAsync();

            // --- ASSERT ---
            // Перевіряємо, що контролер зібрав дані і відправив команду AddBookCommand через Mediator
            _mediatorMock.Verify(m => m.Send(
                It.Is<AddBookCommand>(cmd =>
                    cmd.Dto.Title == "Кобзар" &&
                    cmd.Dto.Author == "Т. Шевченко" &&
                    cmd.Dto.CategoryId == 5
                ),
                It.IsAny<CancellationToken>()
            ), Times.Once);

            // Перевіряємо, що користувачу показали ID створеної книги (100)
            _consoleMock.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("100"))), Times.Once);
        }

        [Fact]
        public async Task DeleteBookAsync_ShouldSendDeleteCommand()
        {
            // --- ARRANGE ---
            // Коли спитають ID, повертаємо "105"
            _consoleMock.Setup(c => c.ReadLine()).Returns("105");

            // --- ACT ---
            await _controller.DeleteBookAsync();

            // --- ASSERT ---
            // Перевіряємо, що Mediator отримав команду DeleteBookCommand з ID = 105
            _mediatorMock.Verify(m => m.Send(
                It.Is<DeleteBookCommand>(cmd => cmd.BookId == 105),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task HandleCommandAsync_ShouldReturnFalse_WhenCommandIsExit()
        {
            // --- ACT ---
            // Відправляємо команду "exit"
            bool result = await _controller.HandleCommandAsync("exit");

            // --- ASSERT ---
            // Метод має повернути false, що є сигналом для зупинки програми
            Assert.False(result);
        }

        [Fact]
        public async Task HandleCommandAsync_ShouldShowHelp_WhenCommandIsHelp()
        {
            // --- ACT ---
            await _controller.HandleCommandAsync("help");

            // --- ASSERT ---
            // Перевіряємо, що контролер вивів список команд (хоча б один рядок з "Доступні команди")
            _consoleMock.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("Доступні команди"))), Times.Once);
        }

        [Fact]
        public async Task HandleCommandAsync_ShouldShowError_WhenCommandIsUnknown()
        {
            // --- ACT ---
            await _controller.HandleCommandAsync("abrakadabra");

            // --- ASSERT ---
            // Перевіряємо, що вивелось повідомлення про невідому команду
            _consoleMock.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("Невідома команда"))), Times.Once);
        }
    }
}