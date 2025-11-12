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
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var book = await _dbContext.Books
                .Include(b => b.Owner)
                .Include(b => b.Category)
                .Include(b => b.Cover)
                .Where(b => b.BookId == request.BookId)
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
                    OwnerId = b.OwnerId,

                    // --- ОНОВЛЕНА ЛОГІКА ДЛЯ ОБКЛАДИНКИ ---
                    CoverPath = (b.Cover == null || string.IsNullOrEmpty(b.Cover.ImagePath))
                        ? null
                        : (b.Cover.ImagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || b.Cover.ImagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            ? b.Cover.ImagePath
                            : Path.Combine(baseDir, b.Cover.ImagePath),

                    CategoryName = (b.Category != null) ? b.Category.Name : "N/A",
                    OwnerFullName = (b.Owner != null) ? (b.Owner.FirstName + " " + b.Owner.LastName) : "N/A",
                    OwnerEmail = (b.Owner != null) ? b.Owner.Email : "N/A",
                    IsFavoritedByCurrentUser = _dbContext.Favorites.Any(f =>
                        f.BookId == b.BookId && f.UserId == request.CurrentUserId)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (book == null)
            {
                throw new Exception($"Книгу з ID {request.BookId} не знайдено.");
            }

            return book;
        }
    }
}