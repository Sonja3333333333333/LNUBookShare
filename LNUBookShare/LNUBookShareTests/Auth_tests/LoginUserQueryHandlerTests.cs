using Xunit;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Auth;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShareTests.Auth
{
    public class LoginUserQueryHandlerTests
    {
        private const int TestFacultyId = 1;
        private const string TestFacultyName = "Тестовий факультет";

        private LNUBookShareDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var context = new LNUBookShareDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private async Task SeedDatabase(LNUBookShareDbContext context)
        {
            context.Faculties.Add(new Faculty { FacultyId = TestFacultyId, Name = TestFacultyName });
            await context.SaveChangesAsync();

            context.Users.Add(new User
            {
                UserId = 100,
                FirstName = "Іван",
                LastName = "Петренко",
                Email = "ivan@lnu.edu.ua",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FacultyId = TestFacultyId,
                IsEmailConfirmed = true
            });

            context.Users.Add(new User
            {
                UserId = 101,
                FirstName = "Неактивний",
                LastName = "Користувач",
                Email = "unconfirmed@lnu.edu.ua",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FacultyId = TestFacultyId,
                IsEmailConfirmed = false
            });
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsLoginResult()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context);

            var handler = new LoginUserQueryHandler(context);
            var query = new LoginUserQuery
            {
                Email = "ivan@lnu.edu.ua",
                Password = "password123"
            };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Іван", result.FirstName);
            Assert.Equal("ivan@lnu.edu.ua", result.Email);
        }

        [Fact]
        public async Task Handle_InvalidPassword_ThrowsException()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context);

            var handler = new LoginUserQueryHandler(context);
            var query = new LoginUserQuery
            {
                Email = "ivan@lnu.edu.ua",
                Password = "wrong"
            };

            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsException()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context);

            var handler = new LoginUserQueryHandler(context);
            var query = new LoginUserQuery
            {
                Email = "nonexistent@lnu.edu.ua",
                Password = "password123"
            };

            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmailNotConfirmed_ThrowsException()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context);

            var handler = new LoginUserQueryHandler(context);
            var query = new LoginUserQuery
            {
                Email = "unconfirmed@lnu.edu.ua",
                Password = "password123"
            };

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(query, CancellationToken.None));
            Assert.Contains("не підтверджено", exception.Message);
        }
    }
}