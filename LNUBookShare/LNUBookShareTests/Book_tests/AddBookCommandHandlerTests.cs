using LNUBookShareBLL.Common;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;

using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareTests.Book_tests
{
    public class AddBookCommandHandlerTests
    {
        private LNUBookShareDbContext _dbContext;
        private DbContextOptions<LNUBookShareDbContext> _options;

        public AddBookCommandHandlerTests()
        {
            this._options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            this._dbContext = new LNUBookShareDbContext(this._options);
        }

        [Fact]
        public async Task Handle_ShouldAddBook_WhenRequestIsValid()
        {
            var path = PathHelper.ConvertToRelativePath("covers/book1.jpg");

            await this._dbContext.Images.AddAsync(new Image
            {
                ImageId = 1,
                ImagePath = path,
                ImageType = "cover",
            });

            await this._dbContext.SaveChangesAsync();

            var handler = new AddBookCommandHandler(this._dbContext);

            var command = new AddBookCommand
            {
                OwnerUserId = 1,
                Dto = new AddBookDto
                {
                    Title = "Test Book",
                    Author = "Author",
                    CategoryId = 1,
                    CoverImagePath = "covers/book1.jpg",
                },
            };

            var result = await handler.Handle(command, CancellationToken.None);

            var addedBook = await this._dbContext.Books.FindAsync(result);
            Assert.NotNull(addedBook);
            Assert.Equal("Test Book", addedBook.Title);
            Assert.Equal("Author", addedBook.Author);
            Assert.Equal(1, addedBook.CategoryId);
            Assert.Equal(1, addedBook.CoverId);
            Assert.Equal("available", addedBook.Status);
            Assert.Equal(1, addedBook.OwnerId);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenTitleIsEmpty()
        {
            var handler = new AddBookCommandHandler(this._dbContext);

            var command = new AddBookCommand
            {
                OwnerUserId = 1,
                Dto = new AddBookDto
                {
                    Title = string.Empty,
                    Author = "Author",
                    CategoryId = 1,
                },
            };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Назва, Автор та Категорія є обов'язковими.", ex.Message);
        }

        [Fact]
        public async Task Handle_ShouldAddBookWithNullCover_WhenImageNotFound()
        {
            var handler = new AddBookCommandHandler(this._dbContext);

            var command = new AddBookCommand
            {
                OwnerUserId = 1,
                Dto = new AddBookDto
                {
                    Title = "Book Without Cover",
                    Author = "Author",
                    CategoryId = 1,
                    CoverImagePath = "nonexistent.jpg",
                },
            };

            var result = await handler.Handle(command, CancellationToken.None);
            var addedBook = await this._dbContext.Books.FindAsync(result);

            Assert.NotNull(addedBook);
            Assert.Null(addedBook.CoverId);
        }
    }
}
