//Handle_ShouldAddFavorite_WhenItDoesNotExist: "Щасливий шлях" №1.Перевіряє, що обробник коректно додає запис Favorite до бази даних, якщо користувач "лайкає" книгу вперше.

//Handle_ShouldRemoveFavorite_WhenItAlreadyExists: "Щасливий шлях" №2.Перевіряє, що обробник коректно видаляє запис Favorite з бази даних, якщо користувач "відлайкує" книгу, яка вже була в обраному.

//Handle_ShouldThrowException_WhenBookDoesNotExist: "Сумний шлях" №1.Перевіряє, що код кидає помилку з повідомленням "Книгу не знайдено.", якщо користувач намагається додати в обране книгу, якої не існує в базі.


using FluentAssertions;

using LNUBookShareBLL.Features.Favorites;

using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareTests.Favorite_tests
{
    public class ToggleFavoriteCommandHandlerTests
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
            var testUser = new User
            {
                UserId = 1,
                FirstName = "TestUser",
                LastName = "Testovych",
                Email = "test@example.com",
                PasswordHash = "dummy_hash_123"
            };
            context.Users.Add(testUser);

            context.Books.Add(new Book
            {
                BookId = 1,
                Title = "Test Book 1",
                OwnerId = testUser.UserId,
                Author = "Test Author 1",
                Status = "available"
            });
            context.Books.Add(new Book
            {
                BookId = 2,
                Title = "Test Book 2",
                OwnerId = testUser.UserId,
                Author = "Test Author 2",
                Status = "available"
            });

            context.Favorites.Add(new Favorite
            {
                FavoriteId = 1,
                UserId = testUser.UserId,
                BookId = 2,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        // === ТЕСТ 1: "Щасливий шлях" (Додавання нового "Обраного") ===
        [Fact]
        public async Task Handle_ShouldAddFavorite_WhenItDoesNotExist()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context);

            var handler = new ToggleFavoriteCommandHandler(context);
            var command = new ToggleFavoriteCommand
            {
                UserId = 1,
                BookId = 1
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().BeTrue();

            var favoriteInDb = await context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == 1 && f.BookId == 1);

            favoriteInDb.Should().NotBeNull();
            (await context.Favorites.CountAsync()).Should().Be(2);
        }

        // === ТЕСТ 2: "Щасливий шлях" (Видалення існуючого "Обраного") ===
        [Fact]
        public async Task Handle_ShouldRemoveFavorite_WhenItAlreadyExists()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context);

            var handler = new ToggleFavoriteCommandHandler(context);
            var command = new ToggleFavoriteCommand
            {
                UserId = 1,
                BookId = 2
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().BeFalse();

            var favoriteInDb = await context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == 1 && f.BookId == 2);

            favoriteInDb.Should().BeNull();
            (await context.Favorites.CountAsync()).Should().Be(0);
        }

        // === ТЕСТ 3: "Сумний шлях" (Книга не знайдена) ===
        [Fact]
        public async Task Handle_ShouldThrowException_WhenBookDoesNotExist()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context);

            var handler = new ToggleFavoriteCommandHandler(context);
            var command = new ToggleFavoriteCommand
            {
                UserId = 1,
                BookId = 999
            };

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("Книгу не знайдено.");

            (await context.Favorites.CountAsync()).Should().Be(1);
        }
    }
}