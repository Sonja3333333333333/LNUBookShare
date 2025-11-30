using Xunit;
using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Auth;


namespace LNUBookShareTests.Auth
{
    public class ResendConfirmationEmailCommandHandlerTests
    {
        private LNUBookShareDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new LNUBookShareDbContext(options);
        }

        private async Task SeedDatabase(LNUBookShareDbContext context, bool isConfirmed, bool hasToken, DateTime? tokenCreationTime = null)
        {
            var faculty = new Faculty { FacultyId = 1, Name = "Тестовий факультет" };
            context.Faculties.Add(faculty);
            await context.SaveChangesAsync();

            var user = new User
            {
                UserId = 101,
                Email = "test@lnu.edu.ua",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "User",
                FacultyId = faculty.FacultyId,
                IsEmailConfirmed = isConfirmed
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            if (hasToken)
            {
                var token = new Emailconfirmation
                {
                    UserId = user.UserId,
                    ConfirmationToken = "test-token-123",
                    CreatedAt = tokenCreationTime ?? DateTime.UtcNow.AddMinutes(-10), // Default: 10 minutes ago
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
                context.Emailconfirmations.Add(token);
                await context.SaveChangesAsync();
            }
        }

        // === ТЕСТ 1: "Щасливий шлях" (Створення нового токена, оскільки старий давно був) ===
        [Fact]
        public async Task Handle_ValidEmail_UpdatesToken()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context, isConfirmed: false, hasToken: true, tokenCreationTime: DateTime.UtcNow.AddHours(-1));

            var handler = new ResendConfirmationEmailCommandHandler(context);
            var command = new ResendConfirmationEmailCommand { Email = "test@lnu.edu.ua" };

            // Зберігаємо старий токен, щоб перевірити, чи він оновився
            var oldToken = await context.Emailconfirmations.FirstAsync();
            var oldCreationTime = oldToken.CreatedAt;

            // ACT
            await handler.Handle(command, CancellationToken.None);

            // ASSERT
            var updatedToken = await context.Emailconfirmations.FirstAsync();
            Assert.NotNull(updatedToken);
            // Перевіряємо, що токен оновився на більш нову дату
            Assert.True(updatedToken.CreatedAt > oldCreationTime);
            Assert.NotEqual("test-token-123", updatedToken.ConfirmationToken);
        }

        // === ТЕСТ 2: "Сумний шлях" (Користувача не знайдено) ===
        [Fact]
        public async Task Handle_UserNotFound_ThrowsException()
        {
            await using var context = this.GetInMemoryDbContext();
            var handler = new ResendConfirmationEmailCommandHandler(context);
            var command = new ResendConfirmationEmailCommand { Email = "nonexistent@lnu.edu.ua" };

            await Assert.ThrowsAsync<Exception>(async () =>
                await handler.Handle(command, CancellationToken.None));
        }

        // === ТЕСТ 3: "Сумний шлях" (Користувач вже підтверджений) ===
        [Fact]
        public async Task Handle_AlreadyConfirmed_ThrowsException()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context, isConfirmed: true, hasToken: false);

            var handler = new ResendConfirmationEmailCommandHandler(context);
            var command = new ResendConfirmationEmailCommand { Email = "test@lnu.edu.ua" };

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await handler.Handle(command, CancellationToken.None));
            Assert.Contains("вже підтверджено", exception.Message);
        }

        // === ТЕСТ 4: БІЗНЕС-ЛОГІКА (Таймаут/Cooldown Check) ===
        [Fact]
        public async Task Handle_TokenOnCooldown_ThrowsException()
        {
            await using var context = this.GetInMemoryDbContext();

            // ARRANGE: Створюємо токен, який був створений 10 секунд тому
            await this.SeedDatabase(context, isConfirmed: false, hasToken: true, tokenCreationTime: DateTime.UtcNow.AddSeconds(-10));

            var handler = new ResendConfirmationEmailCommandHandler(context);
            var command = new ResendConfirmationEmailCommand { Email = "test@lnu.edu.ua" };

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await handler.Handle(command, CancellationToken.None));
            Assert.Contains("лише раз на хвилину", exception.Message);
        }
    }
}