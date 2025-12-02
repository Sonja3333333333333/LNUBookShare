using LNUBookShareBLL.Features.Books;

using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareTests.Book_tests
{
    public class GetBookForEditQueryHandlerTests
    {
        private readonly LNUBookShareDbContext _dbContext;
        private readonly DbContextOptions<LNUBookShareDbContext> _options;

        public GetBookForEditQueryHandlerTests()
        {
            this._options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
                .Options;

            this._dbContext = new LNUBookShareDbContext(this._options);
        }

        [Fact]
        public async Task Handle_ShouldReturnBookEditDto_WhenUserIsOwner()
        {
            // Arrange
            var image = new Image
            {
                ImageId = 1,
                ImagePath = "covers/a.jpg",
                ImageType = "cover",
            };

            var book = new Book
            {
                BookId = 10,
                Title = "Book A",
                Author = "Author",
                Status = "available",
                CategoryId = 2,
                OwnerId = 5,
                CoverId = 1,
            };

            this._dbContext.Images.Add(image);
            this._dbContext.Books.Add(book);
            await this._dbContext.SaveChangesAsync();

            var handler = new GetBookForEditQueryHandler(this._dbContext);

            var query = new GetBookForEditQuery
            {
                BookId = 10,
                CurrentUserId = 5,
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Book A", result.Title);
            Assert.Equal("Author", result.Author);
            Assert.Equal(2, result.CategoryId);
            Assert.Equal("available", result.Status);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenBookNotFound()
        {
            var handler = new GetBookForEditQueryHandler(this._dbContext);

            var query = new GetBookForEditQuery
            {
                BookId = 999,
                CurrentUserId = 1,
            };

            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserIsNotOwner()
        {
            var image = new Image
            {
                ImageId = 2,
                ImagePath = "covers/x.jpg",
                ImageType = "cover",
            };

            var book = new Book
            {
                BookId = 20,
                Title = "Book B",
                Author = "Author B",
                Status = "available",
                CategoryId = 1,
                OwnerId = 7,
                CoverId = 2,
            };

            this._dbContext.Images.Add(image);
            this._dbContext.Books.Add(book);
            await this._dbContext.SaveChangesAsync();

            var handler = new GetBookForEditQueryHandler(this._dbContext);

            var query = new GetBookForEditQuery
            {
                BookId = 20,
                CurrentUserId = 999,
            };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(query, CancellationToken.None));

            Assert.Equal("Ви не можете редагувати чужу книгу.", ex.Message);
        }
    }
}
