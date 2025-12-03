using LNUBookShareBLL.Features.Books;
using LNUBookShareConsole;
using MediatR;
using Moq;

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
            _mediatorMock = new Mock<IMediator>();
            _seederMock = new Mock<IDataSeeder>();
            _consoleMock = new Mock<IConsoleIO>();

            _controller = new ConsoleController(_mediatorMock.Object, _seederMock.Object, _consoleMock.Object);
        }

        [Fact]
        public async Task HandleCommandAsync_ShouldCallSeeder_WhenCommandIsSeed()
        {
            _consoleMock.Setup(c => c.ReadLine()).Returns("50");

            await _controller.HandleCommandAsync("seed");

            _seederMock.Verify(s => s.SeedDatabaseAsync(50), Times.Once);

            _consoleMock.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("успішно"))), Times.Once);
        }

        [Fact]
        public async Task AddBookAsync_ShouldSendMediatorCommand_WithCorrectData()
        {
            _consoleMock.SetupSequence(c => c.ReadLine())
                .Returns("Кобзар")
                .Returns("Т. Шевченко")
                .Returns("5");

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<AddBookCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(100);

            await _controller.AddBookAsync();

            _mediatorMock.Verify(
                m => m.Send(
                It.Is<AddBookCommand>(cmd =>
                    cmd.Dto.Title == "Кобзар" &&
                    cmd.Dto.Author == "Т. Шевченко" &&
                    cmd.Dto.CategoryId == 5),
                It.IsAny<CancellationToken>()), Times.Once);

            _consoleMock.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("100"))), Times.Once);
        }

        [Fact]
        public async Task DeleteBookAsync_ShouldSendDeleteCommand()
        {
            _consoleMock.Setup(c => c.ReadLine()).Returns("105");

            await _controller.DeleteBookAsync();

            _mediatorMock.Verify(
                m => m.Send(
                It.Is<DeleteBookCommand>(cmd => cmd.BookId == 105),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleCommandAsync_ShouldReturnFalse_WhenCommandIsExit()
        {
            bool result = await _controller.HandleCommandAsync("exit");

            Assert.False(result);
        }

        [Fact]
        public async Task HandleCommandAsync_ShouldShowHelp_WhenCommandIsHelp()
        {
            await _controller.HandleCommandAsync("help");

            _consoleMock.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("Доступні команди"))), Times.Once);
        }

        [Fact]
        public async Task HandleCommandAsync_ShouldShowError_WhenCommandIsUnknown()
        {
            await _controller.HandleCommandAsync("abrakadabra");

            _consoleMock.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("Невідома команда"))), Times.Once);
        }
    }
}