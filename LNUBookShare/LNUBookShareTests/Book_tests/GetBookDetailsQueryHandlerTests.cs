using Xunit;
using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Common;

namespace LNUBookShareTests.Book_tests
{

    public class GetBookDetailsQueryHandlerTests
    {
        private readonly LNUBookShareDbContext _dbContext;
        private DbContextOptions<LNUBookShareDbContext> _options;

        public GetBookDetailsQueryHandlerTests()
        {
            this._options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                    .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                    .Options;

            this._dbContext = new LNUBookShareDbContext(this._options);
        }

        [Fact]
        public async Task Handle_ShouldReturnBookDetails_WhenBookExists()
        {
            
            var owner = new User
            {
                UserId = 10,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                PasswordHash = "test_hash"
            };
            var category = new Category { CategoryId = 1, Name = "Fantasy" };
            var image = new Image { ImageId = 5, ImagePath = "covers/test.jpg", ImageType = "cover" };

            var book = new Book
            {
                BookId = 1,
                Title = "Test Book",
                Author = "Author",
                Isbn = "123",
                Year = 2020,
                Publisher = "Pub",
                Language = "EN",
                Status = "available",
                OwnerId = 10,
                CategoryId = 1,
                CoverId = 5
            };

            this._dbContext.Users.Add(owner);
            this._dbContext.Categories.Add(category);
            this._dbContext.Images.Add(image);
            this._dbContext.Books.Add(book);
            await this._dbContext.SaveChangesAsync();

            var handler = new GetBookDetailsQueryHandler(this._dbContext);

            var query = new GetBookDetailsQuery
            {
                BookId = 1,
                CurrentUserId = 10
            };

            
            var result = await handler.Handle(query, CancellationToken.None);

            
            Assert.NotNull(result);
            Assert.Equal(1, result.BookId);
            Assert.Equal("Test Book", result.Title);
            Assert.Equal("Fantasy", result.CategoryName);
            Assert.Equal("John Doe", result.OwnerFullName);
            Assert.Equal("john@test.com", result.OwnerEmail);
            Assert.Equal(PathHelper.ConvertToAbsolutePath("covers/test.jpg"), result.CoverPath);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenBookDoesNotExist()
        {
           
            var handler = new GetBookDetailsQueryHandler(this._dbContext);

            var query = new GetBookDetailsQuery
            {
                BookId = 999,
                CurrentUserId = 1
            };

           
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(query, CancellationToken.None));

            Assert.Equal("Книгу з ID 999 не знайдено.", ex.Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenBookIsFavoritedByUser()
        {
            
            var owner = new User { UserId = 10, FirstName = "Test", LastName = "User", Email = "email@test.com" , PasswordHash = "test_hash" };
            var category = new Category { CategoryId = 1, Name = "Sci-fi" };
            var image = new Image { ImageId = 5, ImagePath = "covers/test.jpg", ImageType = "cover" };

            var book = new Book
            {
                BookId = 2,
                Title = "Fav Book",
                Author = "Author",
                Status = "available",
                OwnerId = 10,
                CategoryId = 1,
                CoverId = 5
            };

            this._dbContext.Users.Add(owner);
            this._dbContext.Categories.Add(category);
            this._dbContext.Images.Add(image);
            this._dbContext.Books.Add(book);

            this._dbContext.Favorites.Add(new Favorite
            {
                FavoriteId = 1,
                BookId = 2,
                UserId = 10
            });

            await this._dbContext.SaveChangesAsync();

            var handler = new GetBookDetailsQueryHandler(this._dbContext);

            var query = new GetBookDetailsQuery
            {
                BookId = 2,
                CurrentUserId = 10
            };

            
            var result = await handler.Handle(query, CancellationToken.None);

            
            Assert.True(result.IsFavoritedByCurrentUser);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenBookIsNotFavoritedByUser()
        {
            
            var owner = new User { UserId = 10, FirstName = "Test", LastName = "User", Email = "email@test.com", PasswordHash = "test_hash" };
            var category = new Category { CategoryId = 1, Name = "Sci-fi" };
            var image = new Image { ImageId = 5, ImagePath = "covers/test.jpg", ImageType = "cover" };

            var book = new Book
            {
                BookId = 3,
                Title = "Not Fav Book",
                Author = "Author",
                Status = "available",
                OwnerId = 10,
                CategoryId = 1,
                CoverId = 5
            };

            this._dbContext.Users.Add(owner);
            this._dbContext.Categories.Add(category);
            this._dbContext.Images.Add(image);
            this._dbContext.Books.Add(book);
            await this._dbContext.SaveChangesAsync();

            var handler = new GetBookDetailsQueryHandler(this._dbContext);

            var query = new GetBookDetailsQuery
            {
                BookId = 3,
                CurrentUserId = 99 
            };

            
            var result = await handler.Handle(query, CancellationToken.None);

            
            Assert.False(result.IsFavoritedByCurrentUser);
        }
    }
}