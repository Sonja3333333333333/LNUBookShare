using LNUBookShareBLL.Common;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;

using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareTests.Book_tests
{
    public class UpdateBookCommandHandlerTests
    {
        private readonly LNUBookShareDbContext _dbContext;
        private DbContextOptions<LNUBookShareDbContext> _options;

        public UpdateBookCommandHandlerTests()
        {
            this._options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            this._dbContext = new LNUBookShareDbContext(this._options);

            this._dbContext.Users.Add(new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PasswordHash = "hash",
            });

            this._dbContext.Books.Add(new Book
            {
                BookId = 1,
                Title = "Original Title",
                Author = "Original Author",
                Status = "available",
                OwnerId = 1,
                CategoryId = 1,
            });

            this._dbContext.Images.Add(new Image
            {
                ImageId = 1,
                ImagePath = PathHelper.ConvertToRelativePath("covers/book1.jpg"),
                ImageType = "cover",
            });

            this._dbContext.SaveChanges();
        }

        [Fact]
        public async Task Handle_ShouldUpdateBook_WhenRequestIsValid()
        {
            var handler = new UpdateBookCommandHandler(this._dbContext);

            var command = new UpdateBookCommand
            {
                CurrentUserId = 1,
                BookId = 1,
                Dto = new BookEditDto
                {
                    Title = "Updated Title",
                    Author = "Updated Author",
                    CategoryId = 2,
                    Status = "issued",
                    CoverImagePath = PathHelper.ConvertToRelativePath("covers/book1.jpg"),
                },
            };

            await handler.Handle(command, CancellationToken.None);

            var updatedBook = await this._dbContext.Books.FindAsync(1);
            Assert.NotNull(updatedBook);
            Assert.Equal("Updated Title", updatedBook.Title);
            Assert.Equal("Updated Author", updatedBook.Author);
            Assert.Equal("issued", updatedBook.Status);
            Assert.Equal(2, updatedBook.CategoryId);
            Assert.Equal(1, updatedBook.CoverId);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenBookDoesNotExist()
        {
            var handler = new UpdateBookCommandHandler(this._dbContext);

            var command = new UpdateBookCommand
            {
                CurrentUserId = 1,
                BookId = 999,
                Dto = new BookEditDto { Title = "New Title" },
            };

            await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserIsNotOwner()
        {
            var handler = new UpdateBookCommandHandler(this._dbContext);

            var command = new UpdateBookCommand
            {
                CurrentUserId = 2,
                BookId = 1,
                Dto = new BookEditDto { Title = "New Title" },
            };

            await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldUpdateBook_WhenCoverDoesNotExist()
        {
            var handler = new UpdateBookCommandHandler(this._dbContext);

            var command = new UpdateBookCommand
            {
                CurrentUserId = 1,
                BookId = 1,
                Dto = new BookEditDto
                {
                    Title = "Updated Title 2",
                    Author = "Updated Author 2",
                    CategoryId = 2,
                    Status = "issued",
                    CoverImagePath = PathHelper.ConvertToAbsolutePath("covers/nonexistent.jpg"),
                },
            };

            await handler.Handle(command, CancellationToken.None);

            var updatedBook = await this._dbContext.Books.FindAsync(1);
            Assert.Equal("Updated Title 2", updatedBook.Title);
            Assert.Equal("Updated Author 2", updatedBook.Author);
            Assert.Equal(2, updatedBook.CategoryId);
            Assert.Null(updatedBook.CoverId);
        }
    }
}
