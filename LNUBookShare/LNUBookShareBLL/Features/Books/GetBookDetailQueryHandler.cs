using MediatR;
using Microsoft.EntityFrameworkCore;
using LNUBookShareBLL.DTOs;
using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Books
{
    public class GetBookDetailsQueryHandler : IRequestHandler<GetBookDetailsQuery, BookDetailsDto>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetBookDetailsQueryHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BookDetailsDto> Handle(GetBookDetailsQuery request, CancellationToken cancellationToken)
        {
            var book = await _dbContext.Books
                // Підтягуємо всі зв'язані дані, які нам потрібні
                .Include(b => b.Owner)
                .Include(b => b.Category)
                .Include(b => b.Cover)
                .Where(b => b.BookId == request.BookId)
                // Проектуємо (Select) у наш DTO
                .Select(b => new BookDetailsDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Author = b.Author,
                    Isbn = b.Isbn,
                    Year = b.Year,
                    Publisher = b.Publisher,
                    Language = b.Language,
                    Status = b.Status,
                    CoverPath = (b.Cover != null) ? b.Cover.ImagePath : null,

                    // Перевірки на null для зв'язаних даних
                    CategoryName = (b.Category != null) ? b.Category.Name : "N/A",
                    OwnerFullName = (b.Owner != null) ? (b.Owner.FirstName + " " + b.Owner.LastName) : "N/A",
                    OwnerEmail = (b.Owner != null) ? b.Owner.Email : "N/A",

                    // Логіка "сердечка"
                    IsFavoritedByCurrentUser = _dbContext.Favorites.Any(f =>
                        f.BookId == b.BookId && f.UserId == request.CurrentUserId)
                })
                .FirstOrDefaultAsync(cancellationToken);

            // Перевірка, чи книга взагалі існує
            if (book == null)
            {
                throw new Exception($"Книгу з ID {request.BookId} не знайдено.");
            }

            return book;
        }
    }
}