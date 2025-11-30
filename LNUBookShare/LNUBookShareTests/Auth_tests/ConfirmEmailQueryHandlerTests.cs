using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Auth;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShareTests.Auth
{
    public class ConfirmEmailCommandHandlerTests
    {
        private LNUBookShareDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var context = new LNUBookShareDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task Handle_ValidToken_ConfirmsEmail()
        {
            await using var context = this.GetInMemoryDbContext();

            var faculty = new Faculty { FacultyId = 1, Name = "Тестовий факультет" };
            context.Faculties.Add(faculty);
            await context.SaveChangesAsync();

            var user = new User
            {
                Email = "test@lnu.edu.ua",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "User",
                FacultyId = faculty.FacultyId,
                IsEmailConfirmed = false
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var token = new Emailconfirmation
            {
                UserId = user.UserId,
                ConfirmationToken = "valid-token",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
            context.Emailconfirmations.Add(token);
            await context.SaveChangesAsync();

            var handler = new ConfirmEmailCommandHandler(context);
            var command = new ConfirmEmailCommand
            {
                ConfirmationToken = "valid-token"
            };

            await handler.Handle(command, CancellationToken.None);

            var updatedUser = await context.Users.FindAsync(user.UserId);
            Assert.True(updatedUser.IsEmailConfirmed);

            var deletedToken = await context.Emailconfirmations
                .FirstOrDefaultAsync(t => t.ConfirmationToken == "valid-token");
            Assert.Null(deletedToken);
        }

        [Fact]
        public async Task Handle_InvalidToken_ThrowsException()
        {
            await using var context = this.GetInMemoryDbContext();
            var handler = new ConfirmEmailCommandHandler(context);
            var command = new ConfirmEmailCommand
            {
                ConfirmationToken = "invalid-token"
            };

            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ExpiredToken_ThrowsException()
        {
            await using var context = this.GetInMemoryDbContext();

            var faculty = new Faculty { FacultyId = 1, Name = "Тестовий факультет" };
            context.Faculties.Add(faculty);
            await context.SaveChangesAsync();

            var user = new User
            {
                Email = "test@lnu.edu.ua",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "User",
                FacultyId = faculty.FacultyId,
                IsEmailConfirmed = false
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var expiredToken = new Emailconfirmation
            {
                UserId = user.UserId,
                ConfirmationToken = "expired-token",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(-1)
            };
            context.Emailconfirmations.Add(expiredToken);
            await context.SaveChangesAsync();

            var handler = new ConfirmEmailCommandHandler(context);
            var command = new ConfirmEmailCommand
            {
                ConfirmationToken = "expired-token"
            };

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
            Assert.Contains("Термін дії токена вийшов", exception.Message);
        }
    }
}