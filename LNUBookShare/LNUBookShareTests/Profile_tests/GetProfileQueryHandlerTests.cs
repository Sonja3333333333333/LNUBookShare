using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Profile;

namespace LNUBookShare.Tests.Profile_tests
{
    public class GetProfileQueryHandlerTests
    {
        private LNUBookShareDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new LNUBookShareDbContext(options);
        }

        private async Task SeedDatabase(LNUBookShareDbContext context, bool includeBooks)
        {
            string urlPath = "https://picsum.photos/640/480/?image=17";

            context.Faculties.Add(new Faculty { FacultyId = 1, Name = "Факультет інформатики" });
            context.Images.AddRange(
                new Image { ImageId = 10, ImagePath = urlPath, ImageType = "avatar", UploadedAt = DateTime.UtcNow },
                new Image { ImageId = 11, ImagePath = urlPath, ImageType = "cover", UploadedAt = DateTime.UtcNow }
            );

            context.Users.Add(new User
            {
                UserId = 101,
                FirstName = "Стожар",
                LastName = "Дмитришина",
                Email = "stozhar@lnu.edu.ua",
                FacultyId = 1,
                AvatarId = 10, 
                PasswordHash = "hash"
            });

            if (includeBooks)
            {
                context.Books.Add(new Book
                {
                    BookId = 1,
                    OwnerId = 101,
                    Title = "Clean Code Book",
                    Author = "R. Martin",
                    Status = "available",
                    CoverId = 11 
                });
            }
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_ShouldReturnFullProfileAndOwnedBooks_WhenUserExists()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context, true); 

            var handler = new GetProfileQueryHandler(context);
            var query = new GetProfileQuery { UserId = 101 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Стожар", result.FirstName);
            Assert.Contains("https://picsum.photos", result.AvatarPath);
            Assert.Single(result.OwnedBooks);
            Assert.Equal("Clean Code Book", result.OwnedBooks.First().Title);
        }

        // === ТЕСТ 2: Крайній випадок (Користувач не має аватара та книг) ===
        [Fact]
        public async Task Handle_ShouldReturnEmptyListsAndNullAvatar_WhenUserIsMinimal()
        {
            await using var context = this.GetInMemoryDbContext();

            context.Faculties.Add(new Faculty { FacultyId = 1, Name = "Факультет" });
            context.Users.Add(new User
            {
                UserId = 102,
                FirstName = "Minimal",
                LastName = "User",
                FacultyId = 1,
                AvatarId = null,
                Email = "test_min@lnu.edu.ua",
                PasswordHash = "dummy_hash"
            });
            await context.SaveChangesAsync();

            var handler = new GetProfileQueryHandler(context);
            var query = new GetProfileQuery { UserId = 102 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Minimal", result.FirstName);
            Assert.Null(result.AvatarPath);
            Assert.Empty(result.OwnedBooks);
        }


        // === ТЕСТ 3: "Сумний шлях" (Користувача не знайдено) ===
        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserDoesNotExist()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context, false);

            var handler = new GetProfileQueryHandler(context);
            var query = new GetProfileQuery { UserId = 999 }; //not exist

            await Assert.ThrowsAsync<System.Exception>(async () =>
                await handler.Handle(query, CancellationToken.None));
        }
    }
}