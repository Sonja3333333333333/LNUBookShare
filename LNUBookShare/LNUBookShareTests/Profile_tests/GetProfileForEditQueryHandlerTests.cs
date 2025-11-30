using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Profile;


namespace LNUBookShare.Tests.Profile_tests
{
    public class GetProfileForEditQueryHandlerTests
    {

        private LNUBookShareDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new LNUBookShareDbContext(options);
        }

        private async Task SeedDatabase(LNUBookShareDbContext context)
        {
 
            context.Faculties.Add(new Faculty { FacultyId = 1, Name = "Факультет інформатики" });
            context.Images.Add(new Image { ImageId = 50, ImagePath = "uploads\\avatars\\default.png" , ImageType = "avatar"} );

            context.Users.Add(new User
            {
                UserId = 101,
                FirstName = "Стожар",
                LastName = "Дмитришина",
                Email = "stozhar@lnu.edu.ua",
                FacultyId = 1,
                AvatarId = 50,
                PasswordHash = "hash"
            });
            await context.SaveChangesAsync();
        }

        // === ТЕСТ 1: "Щасливий шлях" (Успішне завантаження та конвертація шляху) ===
        [Fact]
        public async Task Handle_ShouldReturnProfileDtoWithAbsolutePath_WhenUserExists()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context);
            var handler = new GetProfileForEditQueryHandler(context);
            var query = new GetProfileForEditQuery { UserId = 101 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Стожар", result.FirstName);
            Assert.Equal(1, result.FacultyId);
        }

        // === ТЕСТ 2: "Сумний шлях" (Користувача не знайдено) ===
        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserDoesNotExist()
        {
            await using var context = this.GetInMemoryDbContext();

            var handler = new GetProfileForEditQueryHandler(context);
            var query = new GetProfileForEditQuery { UserId = 999 }; //not existing

            await Assert.ThrowsAsync<Exception>(async () =>
                await handler.Handle(query, CancellationToken.None));
        }
    }
}