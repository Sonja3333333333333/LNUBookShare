using Xunit;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Auth;
using LNUBookShareBLL.DTOs;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace LNUBookShareTests.Auth
{
    public class LoginUserQueryHandlerTests : TestBase
    {
        [Fact]
        public async Task Handle_ValidCredentials_ReturnsLoginResult()
        {
            // Arrange
            var faculty = new Faculty { Name = "Тестовий факультет" };
            DbContext.Faculties.Add(faculty);
            await DbContext.SaveChangesAsync();

            var user = new User
            {
                FirstName = "Іван",
                LastName = "Петренко",
                Email = "ivan@lnu.edu.ua",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FacultyId = faculty.FacultyId,
                IsEmailConfirmed = true
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var handler = new LoginUserQueryHandler(DbContext);
            var query = new LoginUserQuery
            {
                Email = "ivan@lnu.edu.ua",
                Password = "password123"
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Іван", result.FirstName);
            Assert.Equal("ivan@lnu.edu.ua", result.Email);
        }

        [Fact]
        public async Task Handle_InvalidPassword_ThrowsException()
        {
            // Arrange
            var faculty = new Faculty { Name = "Тестовий факультет" };
            DbContext.Faculties.Add(faculty);
            await DbContext.SaveChangesAsync();

            var user = new User
            {
                Email = "test@lnu.edu.ua",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct"),
                IsEmailConfirmed = true,
                FacultyId = faculty.FacultyId,
                FirstName = "Тест",
                LastName = "Тестович"
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var handler = new LoginUserQueryHandler(DbContext);
            var query = new LoginUserQuery
            {
                Email = "test@lnu.edu.ua",
                Password = "wrong"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsException()
        {
            // Arrange
            var handler = new LoginUserQueryHandler(DbContext);
            var query = new LoginUserQuery
            {
                Email = "nonexistent@lnu.edu.ua",
                Password = "password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmailNotConfirmed_ThrowsException()
        {
            // Arrange
            var faculty = new Faculty { Name = "Тестовий факультет" };
            DbContext.Faculties.Add(faculty);
            await DbContext.SaveChangesAsync();

            var user = new User
            {
                Email = "unconfirmed@lnu.edu.ua",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                IsEmailConfirmed = false,
                FacultyId = faculty.FacultyId,
                FirstName = "Неактивний",
                LastName = "Користувач"
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var handler = new LoginUserQueryHandler(DbContext);
            var query = new LoginUserQuery
            {
                Email = "unconfirmed@lnu.edu.ua",
                Password = "password123"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(query, CancellationToken.None));
            Assert.Contains("не підтверджено", exception.Message);
        }
    }
}