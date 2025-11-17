using System;
using System.Threading;
using System.Threading.Tasks;

using LNUBookShareBLL.Features.Auth;

using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace LNUBookShareTests.Auth
{
    public class RegisterUserCommandHandlerTests : TestBase
    {
        [Fact]
        public async Task Handle_ValidData_CreatesUserAndToken()
        {
            // Arrange
            var faculty = new Faculty { Name = "Тестовий факультет" };
            DbContext.Faculties.Add(faculty);
            await DbContext.SaveChangesAsync();

            var handler = new RegisterUserCommandHandler(DbContext);
            var command = new RegisterUserCommand
            {
                FirstName = "Петро",
                LastName = "Іваненко",
                Email = "petro@lnu.edu.ua",
                Password = "password123",
                FacultyId = faculty.FacultyId
            };

            // Act
            var userId = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(userId > 0);

            var user = await DbContext.Users.FindAsync(userId);
            Assert.NotNull(user);
            Assert.Equal("Петро", user.FirstName);
            Assert.Equal("petro@lnu.edu.ua", user.Email);
            Assert.False(user.IsEmailConfirmed);

            var token = await DbContext.Emailconfirmations
                .FirstOrDefaultAsync(t => t.UserId == userId);
            Assert.NotNull(token);
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ThrowsException()
        {
            // Arrange
            var faculty = new Faculty { Name = "Тестовий факультет" };
            DbContext.Faculties.Add(faculty);
            await DbContext.SaveChangesAsync();

            var existingUser = new User
            {
                Email = "existing@lnu.edu.ua",
                PasswordHash = "hash",
                FirstName = "Existing",
                LastName = "User",
                FacultyId = faculty.FacultyId,
                IsEmailConfirmed = true
            };
            DbContext.Users.Add(existingUser);
            await DbContext.SaveChangesAsync();

            var handler = new RegisterUserCommandHandler(DbContext);
            var command = new RegisterUserCommand
            {
                FirstName = "New",
                LastName = "User",
                Email = "existing@lnu.edu.ua",
                Password = "password123",
                FacultyId = faculty.FacultyId
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
            Assert.Contains("уже зареєстрований", exception.Message);
        }

        [Fact]
        public async Task Handle_InvalidEmail_ThrowsException()
        {
            // Arrange
            var faculty = new Faculty { Name = "Тестовий факультет" };
            DbContext.Faculties.Add(faculty);
            await DbContext.SaveChangesAsync();

            var handler = new RegisterUserCommandHandler(DbContext);
            var command = new RegisterUserCommand
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@gmail.com", 
                Password = "password123",
                FacultyId = faculty.FacultyId
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShortPassword_ThrowsException()
        {
            // Arrange
            var faculty = new Faculty { Name = "Тестовий факультет" };
            DbContext.Faculties.Add(faculty);
            await DbContext.SaveChangesAsync();

            var handler = new RegisterUserCommandHandler(DbContext);
            var command = new RegisterUserCommand
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@lnu.edu.ua",
                Password = "short", 
                FacultyId = faculty.FacultyId
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
        }
    }
}