
using LNUBookShareBLL;
using LNUBookShareBLL.Features.Auth;

using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;

using Moq;




namespace LNUBookShareTests.Auth
{
    public class RegisterUserCommandHandlerTests
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

        private async Task SeedFaculty(LNUBookShareDbContext context)
        {
            var faculty = new Faculty { FacultyId = 1, Name = "Тестовий факультет" };
            context.Faculties.Add(faculty);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_ValidData_CreatesUserAndToken()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedFaculty(context);

            var mockEmailService = new Mock<IEmailService>();

            var handler = new RegisterUserCommandHandler(context, mockEmailService.Object);
            var command = new RegisterUserCommand
            {
                FirstName = "Петро",
                LastName = "Іваненко",
                Email = "petro@lnu.edu.ua",
                Password = "password123",
                FacultyId = 1
            };

            var userId = await handler.Handle(command, CancellationToken.None);

            Assert.True(userId > 0);
            var user = await context.Users.FindAsync(userId);
            Assert.NotNull(user);
            Assert.Equal("Петро", user.FirstName);
            Assert.Equal("petro@lnu.edu.ua", user.Email);
            Assert.False(user.IsEmailConfirmed);

            var token = await context.Emailconfirmations
                .FirstOrDefaultAsync(t => t.UserId == userId);
            Assert.NotNull(token);
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ThrowsException()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedFaculty(context);

            var existingUser = new User
            {
                Email = "existing@lnu.edu.ua",
                PasswordHash = "hash",
                FirstName = "Existing",
                LastName = "User",
                FacultyId = 1,
                IsEmailConfirmed = true
            };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var mockEmailService = new Mock<IEmailService>();

            var handler = new RegisterUserCommandHandler(context, mockEmailService.Object);
            var command = new RegisterUserCommand
            {
                FirstName = "New",
                LastName = "User",
                Email = "existing@lnu.edu.ua",
                Password = "password123",
                FacultyId = 1
            };

            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_InvalidEmail_ThrowsException()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedFaculty(context);

            var mockEmailService = new Mock<IEmailService>();

            var handler = new RegisterUserCommandHandler(context, mockEmailService.Object);
            var command = new RegisterUserCommand
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@gmail.com",
                Password = "password123",
                FacultyId = 1
            };

            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShortPassword_ThrowsException()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedFaculty(context);

            var mockEmailService = new Mock<IEmailService>();

            var handler = new RegisterUserCommandHandler(context, mockEmailService.Object);
            var command = new RegisterUserCommand
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@lnu.edu.ua",
                Password = "short",
                FacultyId = 1
            };

            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
        }
    }
}