using MediatR;
using Microsoft.EntityFrameworkCore;
using LNUBookShareBLL.DTOs;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Common;


namespace LNUBookShareBLL.Features.Books
{
    public class GetBookDetailsQueryHandler : IRequestHandler<GetBookDetailsQuery, BookDetailsDto>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetBookDetailsQueryHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<BookDetailsDto> Handle(GetBookDetailsQuery request, CancellationToken cancellationToken)
        {
            var bookDto = await this.GetBookDetailsDtoAsync(request, cancellationToken);

            if (bookDto == null)
            {
                throw new Exception($"Книгу з ID {request.BookId} не знайдено.");
            }

            return bookDto;
        }

        private async Task<BookDetailsDto> GetBookDetailsDtoAsync(GetBookDetailsQuery request, CancellationToken cancellationToken)
        {
            return await this._dbContext.Books
                .AsNoTracking()
                .Where(book => book.BookId == request.BookId)
                .Include(book => book.Owner)
                .Include(book => book.Category)
                .Include(book => book.Cover)
                .Select(book => new BookDetailsDto
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    Isbn = book.Isbn,
                    Year = book.Year,
                    Publisher = book.Publisher,
                    Language = book.Language,
                    Status = book.Status,
                    OwnerId = book.OwnerId,
                    CoverPath = PathHelper.ConvertToAbsolutePath(book.Cover != null ? book.Cover.ImagePath : null),
                    CategoryName = (book.Category != null) ? book.Category.Name : "N/A",
                    OwnerFullName = (book.Owner != null) ? (book.Owner.FirstName + " " + book.Owner.LastName) : "N/A",
                    OwnerEmail = (book.Owner != null) ? book.Owner.Email : "N/A",
                    IsFavoritedByCurrentUser = this._dbContext.Favorites.Any(favorite =>
                        favorite.BookId == book.BookId && favorite.UserId == request.CurrentUserId)
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}