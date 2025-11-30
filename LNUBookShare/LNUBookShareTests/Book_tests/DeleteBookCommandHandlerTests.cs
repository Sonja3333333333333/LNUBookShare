using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using LNUBookShareDAL.Models;
using LNUBookShareDAL;
using LNUBookShareBLL.Features.Books;

namespace LNUBookShareTests.Book_tests
{
    public class DeleteBookCommandHandlerTests
    {
        private readonly LNUBookShareDbContext _dbContext;
        private readonly DbContextOptions<LNUBookShareDbContext> _options;

        public DeleteBookCommandHandlerTests()
        {
            this._options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            this._dbContext = new LNUBookShareDbContext(this._options);
        }

        
        [Fact]
        public async Task Handle_ShouldDeleteBook_WhenUserIsOwner()
        {
            
            var book = new Book
            {
                BookId = 1,
                Title = "Test",
                Author = "Author",
                CategoryId = 1,
                Status = "available",
                OwnerId = 10
            };

            this._dbContext.Books.Add(book);
            await this._dbContext.SaveChangesAsync();

            var handler = new DeleteBookCommandHandler(this._dbContext);
            var command = new DeleteBookCommand
            {
                BookId = 1,
                CurrentUserId = 10
            };

            
            await handler.Handle(command, CancellationToken.None);

            
            var deleted = await this._dbContext.Books.FindAsync(1);
            Assert.Null(deleted); 
        }

        
        [Fact]
        public async Task Handle_ShouldThrowException_WhenBookNotFound()
        {
            
            var handler = new DeleteBookCommandHandler(this._dbContext);
            var command = new DeleteBookCommand
            {
                BookId = 999,   
                CurrentUserId = 5
            };
            
            var ex = await Assert.ThrowsAsync<System.Exception>(() =>
                handler.Handle(command, CancellationToken.None));
            
            Assert.Equal("Книгу не знайдено.", ex.Message);
        }

        
        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserIsNotOwner()
        {
            
            var book = new Book
            {
                BookId = 10,
                Title = "Foreign Book",
                OwnerId = 7,                
                Author = "Author",
                CategoryId = 1,
                Status = "available"                
            };

            this._dbContext.Books.Add(book);
            await this._dbContext.SaveChangesAsync();

            var handler = new DeleteBookCommandHandler(this._dbContext);
            var command = new DeleteBookCommand
            {
                BookId = 10,
                CurrentUserId = 5 
            };
            
            var ex = await Assert.ThrowsAsync<System.Exception>(() =>
                handler.Handle(command, CancellationToken.None));
            
            Assert.Equal("Ви не можете видалити книгу, яка вам не належить.", ex.Message);
        }
    }
}
