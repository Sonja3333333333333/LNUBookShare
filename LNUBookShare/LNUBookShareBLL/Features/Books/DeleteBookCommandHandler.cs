using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareBLL.Features.Books
{
    public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public DeleteBookCommandHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Unit> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
        {
            var book = await this.GetBookAsync(request.BookId, cancellationToken);

            this.ValidateOwnership(book, request.CurrentUserId);

            await this.DeleteBookAndSaveAsync(book, cancellationToken);

            return Unit.Value;
        }

        private async Task<Book> GetBookAsync(int bookId, CancellationToken cancellationToken)
        {
            var book = await this._dbContext.Books
                .FirstOrDefaultAsync(book => book.BookId == bookId, cancellationToken);

            if (book == null)
            {
                throw new System.Exception("Книгу не знайдено.");
            }

            return book;
        }

        private void ValidateOwnership(Book book, int currentUserId)
        {
            if (book.OwnerId != currentUserId)
            {
                throw new System.Exception("Ви не можете видалити книгу, яка вам не належить.");
            }
        }

        private async Task DeleteBookAndSaveAsync(Book book, CancellationToken cancellationToken)
        {
            this._dbContext.Books.Remove(book);
            await this._dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}