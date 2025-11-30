//Handle_ShouldUpdateUserProfile_WhenDataIsValid: "Щасливий шлях" №1.Перевіряє, що ім'я, прізвище та факультет оновлюються, а аватар не змінюється, якщо передати string.Empty.

//Handle_ShouldUpdateUserAvatar_WhenProfileImageUrlIsValid: "Щасливий шлях" №2.Перевіряє, що AvatarId користувача оновлюється на 99, коли ми передаємо валідний шлях до зображення з нашої "бази".

//Handle_ShouldThrowException_WhenUserNotFound: "Сумний шлях" №1.Перевіряє, що код кидає помилку, якщо UserId не знайдено.

//[Theory] (Теорії): Це тести, які запускаються кілька разів з різними даними (InlineData). Це дозволяє нам перевірити всю логіку валідації трьома невеликими тестами, замість того, щоб писати 7 окремих тестів.

//Ми перевіряємо, що код кидає правильні помилки на неправильні FirstName, LastName та FacultyId.


using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Profile;
using LNUBookShareBLL.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LNUBookShareBLL.Tests.Profile
{
    public class UpdateProfileCommandHandlerTests
    {
        private LNUBookShareDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new LNUBookShareDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private async Task SeedDatabase(LNUBookShareDbContext context)
        {
            context.Faculties.Add(new Faculty { FacultyId = 1, Name = "Факультет 1" });
            context.Faculties.Add(new Faculty { FacultyId = 2, Name = "Факультет 2" });

            context.Images.Add(new Image
            {
                ImageId = 99,
                ImagePath = @"uploads\images\test-avatar.png",
                ImageType = ".png",
                UploadedAt = DateTime.UtcNow
            });

            context.Users.Add(new User
            {
                UserId = 1,
                FirstName = "OldFirstName",
                LastName = "OldLastName",
                Email = "test@example.com",
                PasswordHash = "dummy_hash",
                FacultyId = 1, 
                AvatarId = 50  
            });

            await context.SaveChangesAsync();
        }

        // === "Щасливі шляхи"  ===

        [Fact]
        public async Task Handle_ShouldUpdateUserProfile_WhenDataIsValid()
        {
            await using var context = GetInMemoryDbContext();
            await SeedDatabase(context);

            var handler = new UpdateProfileCommandHandler(context);
            var command = new UpdateProfileCommand
            {
                UserId = 1,
                Dto = new ProfileEditDto
                {
                    FirstName = "NewName",
                    LastName = "NewLastName",
                    FacultyId = 2, 
                    ProfileImageUrl = string.Empty 
                }
            };

            await handler.Handle(command, CancellationToken.None);

            var userInDb = await context.Users.FindAsync(1);
            userInDb.Should().NotBeNull();
            userInDb.FirstName.Should().Be("NewName");
            userInDb.LastName.Should().Be("NewLastName");
            userInDb.FacultyId.Should().Be(2); 
            userInDb.AvatarId.Should().Be(50); 
            userInDb.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task Handle_ShouldUpdateUserAvatar_WhenProfileImageUrlIsValid()
        {
            await using var context = GetInMemoryDbContext();
            await SeedDatabase(context);

            var handler = new UpdateProfileCommandHandler(context);
            var command = new UpdateProfileCommand
            {
                UserId = 1,
                Dto = new ProfileEditDto
                {
                    FirstName = "NewName",
                    LastName = "NewLastName",
                    FacultyId = 2,
                    ProfileImageUrl = @"uploads\images\test-avatar.png" 
                }
            };

            await handler.Handle(command, CancellationToken.None);

            var userInDb = await context.Users.FindAsync(1);
            userInDb.Should().NotBeNull();
            userInDb.AvatarId.Should().Be(99); 
        }

        // === "Сумні шляхи"  ===

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserNotFound()
        {
            await using var context = GetInMemoryDbContext();
            await SeedDatabase(context);

            var handler = new UpdateProfileCommandHandler(context);
            var command = new UpdateProfileCommand
            {
                UserId = 999,
                Dto = new ProfileEditDto { FirstName = "Valid", LastName = "Valid", FacultyId = 1 }
            };

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("Користувача не знайдено.");
        }

        [Theory]
        [InlineData("Name123")] 
        [InlineData(" ")]       
        [InlineData(null)]      
        public async Task Handle_ShouldThrowException_WhenFirstNameIsInvalid(string invalidFirstName)
        {
            await using var context = GetInMemoryDbContext();
            var handler = new UpdateProfileCommandHandler(context);
            var command = new UpdateProfileCommand
            {
                UserId = 1,
                Dto = new ProfileEditDto { FirstName = invalidFirstName, LastName = "Valid", FacultyId = 1 }
            };

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("Ім'я повинно містити лише літери.");
        }

        [Theory]
        [InlineData("Last123")] 
        [InlineData(" ")]       
        [InlineData(null)]      
        public async Task Handle_ShouldThrowException_WhenLastNameIsInvalid(string invalidLastName)
        {
            await using var context = GetInMemoryDbContext();
            var handler = new UpdateProfileCommandHandler(context);
            var command = new UpdateProfileCommand
            {
                UserId = 1,
                Dto = new ProfileEditDto { FirstName = "Valid", LastName = invalidLastName, FacultyId = 1 }
            };

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("Прізвище повинно містити лише літери.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Handle_ShouldThrowException_WhenFacultyIdIsInvalid(int invalidFacultyId)
        {
            await using var context = GetInMemoryDbContext();
            var handler = new UpdateProfileCommandHandler(context);
            var command = new UpdateProfileCommand
            {
                UserId = 1,
                Dto = new ProfileEditDto { FirstName = "Valid", LastName = "Valid", FacultyId = invalidFacultyId }
            };

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("Необхідно обрати факультет.");
        }
    }
}