using LNUBookShareBLL.Common;
using LNUBookShareBLL.DTOs;

using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;


namespace LNUBookShareBLL.Features.Books
{
    public class GetBookForEditQueryHandler : IRequestHandler<GetBookForEditQuery, BookEditDto>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetBookForEditQueryHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<BookEditDto> Handle(GetBookForEditQuery request, CancellationToken cancellationToken)
        {
            var book = await this.GetBookByIdAsync(request.BookId, cancellationToken);

            this.ValidateOwnership(book, request.CurrentUserId);

            return this.MapBookToDto(book);
        }

        private async Task<Book> GetBookByIdAsync(int bookId, CancellationToken cancellationToken)
        {
            var book = await this._dbContext.Books
                .AsNoTracking()
                .Include(book => book.Cover)
                .Where(book => book.BookId == bookId)
                .FirstOrDefaultAsync(cancellationToken);

            if (book == null)
            {
                throw new Exception("Книгу не знайдено.");
            }

            return book;
        }

        private void ValidateOwnership(Book book, int currentUserId)
        {
            if (book.OwnerId != currentUserId)
            {
                throw new Exception("Ви не можете редагувати чужу книгу.");
            }
        }

        private BookEditDto MapBookToDto(Book book)
        {
            return new BookEditDto
            {
                Title = book.Title,
                Author = book.Author,
                Isbn = book.Isbn,
                Year = book.Year,
                Publisher = book.Publisher,
                Language = book.Language,
                CategoryId = book.CategoryId,
                Status = book.Status,
                CoverImagePath = PathHelper.ConvertToAbsolutePath(book.Cover != null ? book.Cover.ImagePath : null)
            };
        }
    }
}