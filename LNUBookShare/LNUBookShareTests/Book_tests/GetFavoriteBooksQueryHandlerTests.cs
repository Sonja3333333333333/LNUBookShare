using LNUBookShareBLL.Enums;
using LNUBookShareBLL.Features.Favorites;

using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareTests.Book_tests
{
    public class GetFavoriteBooksQueryHandlerTests
    {
        private readonly LNUBookShareDbContext _dbContext;
        private DbContextOptions<LNUBookShareDbContext> _options;

        public GetFavoriteBooksQueryHandlerTests()
        {
            this._options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            this._dbContext = new LNUBookShareDbContext(this._options);

            var user = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PasswordHash = "hash",
            };

            var category = new Category
            {
                CategoryId = 1,
                Name = "Programming",
            };

            this._dbContext.Users.Add(user);
            this._dbContext.Categories.Add(category);
            this._dbContext.SaveChanges();

            var book1 = new Book { BookId = 1, Title = "C# Basics", Author = "John Doe", Status = "available", OwnerId = 1, CategoryId = 1 };
            var book2 = new Book { BookId = 2, Title = "Advanced C#", Author = "Jane Doe", Status = "issued", OwnerId = 1, CategoryId = 1 };
            var book3 = new Book { BookId = 3, Title = "Entity Framework", Author = "John Doe", Status = "available", OwnerId = 1, CategoryId = 1 };

            this._dbContext.Books.AddRange(book1, book2, book3);
            this._dbContext.SaveChanges();

            this._dbContext.Favorites.AddRange(
                new Favorite { FavoriteId = 1, UserId = 1, BookId = 1 },
                new Favorite { FavoriteId = 2, UserId = 1, BookId = 2 },
                new Favorite { FavoriteId = 3, UserId = 1, BookId = 3 });
            this._dbContext.SaveChanges();
        }

        [Fact]
        public async Task Handle_ShouldReturnAllFavorites_WhenNoFiltersApplied()
        {
            var handler = new GetFavoriteBooksQueryHandler(this._dbContext);
            var query = new GetFavoriteBooksQuery
            {
                CurrentUserId = 1,
                PageNumber = 1,
                PageSize = 10,
            };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count);
        }

        [Fact]
        public async Task Handle_ShouldFilterByAvailableStatus()
        {
            var handler = new GetFavoriteBooksQueryHandler(this._dbContext);
            var query = new GetFavoriteBooksQuery
            {
                CurrentUserId = 1,
                FilterBy = BookFilterStatus.Available,
                PageNumber = 1,
                PageSize = 10,
            };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, book => Assert.Equal("available", book.Status));
        }

        [Fact]
        public async Task Handle_ShouldApplySortingByAuthor()
        {
            var handler = new GetFavoriteBooksQueryHandler(this._dbContext);
            var query = new GetFavoriteBooksQuery
            {
                CurrentUserId = 1,
                SortBy = BookSortCriteria.Author,
                PageNumber = 1,
                PageSize = 10,
            };

            var result = await handler.Handle(query, CancellationToken.None);

            var authors = result.Items.Select(b => b.Author).ToList();
            var sortedAuthors = authors.OrderBy(a => a).ToList();
            Assert.Equal(sortedAuthors, authors);
        }

        [Fact]
        public async Task Handle_ShouldApplyPagination()
        {
            var handler = new GetFavoriteBooksQueryHandler(this._dbContext);
            var query = new GetFavoriteBooksQuery
            {
                CurrentUserId = 1,
                PageNumber = 2,
                PageSize = 2,
            };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(3, result.TotalCount);
            Assert.Single(result.Items);
        }
    }
}
