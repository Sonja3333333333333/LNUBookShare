using Xunit;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Auth;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShareTests.Auth
{
    public class ResendConfirmationEmailCommandHandlerTests : TestBase
    {
        [Fact]
        public async Task Handle_ValidEmail_CreatesNewToken()
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

            var oldToken = new Emailconfirmation
            {
                UserId = user.UserId,
                ConfirmationToken = "old-token",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
            DbContext.Emailconfirmations.Add(oldToken);
            await DbContext.SaveChangesAsync();

            var handler = new ResendConfirmationEmailCommandHandler(DbContext);
            var command = new ResendConfirmationEmailCommand
            {
                Email = "test@lnu.edu.ua"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedToken = await DbContext.Emailconfirmations
                .FirstOrDefaultAsync(t => t.UserId == user.UserId);
            Assert.NotNull(updatedToken);
            Assert.NotEqual("old-token", updatedToken.ConfirmationToken);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsException()
        {
            // Arrange
            var handler = new ResendConfirmationEmailCommandHandler(DbContext);
            var command = new ResendConfirmationEmailCommand
            {
                Email = "nonexistent@lnu.edu.ua"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_AlreadyConfirmed_ThrowsException()
        {
            // Arrange
            var faculty = new Faculty { Name = "Тестовий факультет" };
            DbContext.Faculties.Add(faculty);
            await DbContext.SaveChangesAsync();

            var user = new User
            {
                Email = "confirmed@lnu.edu.ua",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "User",
                FacultyId = faculty.FacultyId,
                IsEmailConfirmed = true 
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var handler = new ResendConfirmationEmailCommandHandler(DbContext);
            var command = new ResendConfirmationEmailCommand
            {
                Email = "confirmed@lnu.edu.ua"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));
            Assert.Contains("вже підтверджено", exception.Message);
        }
    }
}