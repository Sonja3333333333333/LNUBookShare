using Xunit;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Auth;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShareTests.Auth
{
    public class ConfirmEmailCommandHandlerTests : TestBase
    {
        [Fact]
        public async Task Handle_ValidToken_ConfirmsEmail()
        {
            // Arrange
            var faculty = new Faculty { Name = "Тестовий факультет" };
            DbContext.Faculties.Add(faculty);
            await DbContext.SaveChangesAsync();

            var user = new User
            {
                Email = "test@lnu.edu.ua",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "User",
                FacultyId = faculty.FacultyId,
                IsEmailConfirmed = false
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var token = new Emailconfirmation
            {
                UserId = user.UserId,
                ConfirmationToken = "valid-token",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
            DbContext.Emailconfirmations.Add(token);
            await DbContext.SaveChangesAsync();

            var handler = new ConfirmEmailCommandHandler(DbContext);
            var command = new ConfirmEmailCommand
            {
                ConfirmationToken = "valid-token"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedUser = await DbContext.Users.FindAsync(user.UserId);
            Assert.True(updatedUser.IsEmailConfirmed);

            var deletedToken = await DbContext.Emailconfirmations
                .FirstOrDefaultAsync(t => t.ConfirmationToken == "valid-token");
            Assert.Null(deletedToken);
        }

        [Fact]
        public async Task Handle_InvalidToken_ThrowsException()
        {
            // Arrange
            var handler = new ConfirmEmailCommandHandler(DbContext);
            var command = new ConfirmEmailCommand
            {
                ConfirmationToken = "invalid-token"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ExpiredToken_ThrowsException()
        {
            // Arrange
            var faculty = new Faculty { Name = "Тестовий факультет" };
            DbContext.Faculties.Add(faculty);
            await DbContext.SaveChangesAsync();

            var user = new User
            {
                Email = "test@lnu.edu.ua",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "User",
                FacultyId = faculty.FacultyId,
                IsEmailConfirmed = false
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var expiredToken = new Emailconfirmation
            {
                UserId = user.UserId,
                ConfirmationToken = "expired-token",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(-1) 
            };
            DbContext.Emailconfirmations.Add(expiredToken);
            await DbContext.SaveChangesAsync();

            var handler = new ConfirmEmailCommandHandler(DbContext);
            var command = new ConfirmEmailCommand
            {
                ConfirmationToken = "expired-token"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
            Assert.Contains("Термін дії токена вийшов", exception.Message);
        }
    }
}