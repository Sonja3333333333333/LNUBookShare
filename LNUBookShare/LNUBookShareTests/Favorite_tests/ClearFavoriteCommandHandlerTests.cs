using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Favorites;
using MediatR; 

namespace LNUBookShare.Tests.Favorite_tests
{
    public class ClearFavoritesCommandHandlerTests
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

        private async Task SeedDatabase(LNUBookShareDbContext context)
        {
            var user1 = new User { UserId = 101, FirstName = "User1", LastName = "Test1", Email = "u1@test.ua", PasswordHash = "hash" };
            var user2 = new User { UserId = 102, FirstName = "User2", LastName = "Test2", Email = "u2@test.ua", PasswordHash = "hash" };

            context.Users.AddRange(user1, user2);

            context.Books.AddRange(
                new Book { BookId = 1, OwnerId = 101, Title = "Book A", Author = "A", Status = "available" },
                new Book { BookId = 2, OwnerId = 101, Title = "Book B", Author = "B", Status = "available"},
                new Book { BookId = 3, OwnerId = 102, Title = "Book C", Author = "C", Status = "available" }
            );

            context.Favorites.AddRange(
                new Favorite { UserId = 101, BookId = 1 },
                new Favorite { UserId = 101, BookId = 2 },
                new Favorite { UserId = 102, BookId = 3 } 
            );

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_ShouldRemoveAllFavorites_WhenFavoritesExist()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context);
            var handler = new ClearFavoritesCommandHandler(context);
            var command = new ClearFavoritesCommand { UserId = 101 }; 

            Assert.Equal(3, await context.Favorites.CountAsync());
            Assert.Equal(2, await context.Favorites.CountAsync(f => f.UserId == 101));


            var result = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result); 
            Assert.Equal(1, await context.Favorites.CountAsync());
            Assert.False(await context.Favorites.AnyAsync(f => f.UserId == 101));
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenNoFavoritesExist()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedDatabase(context);

            var handler = new ClearFavoritesCommandHandler(context);
            var command = new ClearFavoritesCommand { UserId = 103 };

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            Assert.Equal(3, await context.Favorites.CountAsync());
        }
    }
}