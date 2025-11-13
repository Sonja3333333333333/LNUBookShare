using MediatR;
using Microsoft.EntityFrameworkCore;
using LNUBookShareBLL.DTOs;
using LNUBookShareDAL.Models;

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
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var bookData = await this._dbContext.Books
                .AsNoTracking()
                .Include(b => b.Cover) // <-- ВАЖЛИВО: Завантажуємо обкладинку
                .Where(b => b.BookId == request.BookId)
                .Select(b => new
                {
                    BookData = new BookEditDto
                    {
                        Title = b.Title,
                        Author = b.Author,
                        Isbn = b.Isbn,
                        Year = b.Year,
                        Publisher = b.Publisher,
                        Language = b.Language,
                        CategoryId = b.CategoryId,
                        Status = b.Status,

                        // --- ОНОВЛЕНА ЛОГІКА ДЛЯ ФОТО ---
                        CoverImagePath = (b.Cover == null || string.IsNullOrEmpty(b.Cover.ImagePath))
                            ? null // Немає шляху
                            : (b.Cover.ImagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || b.Cover.ImagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                                ? b.Cover.ImagePath // Це URL
                                : Path.Combine(baseDir, b.Cover.ImagePath) // Це файл
                    },
                    OwnerId = b.OwnerId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (bookData == null)
            {
                throw new Exception("Книгу не знайдено.");
            }

            if (bookData.OwnerId != request.CurrentUserId)
            {
                throw new Exception("Ви не можете редагувати чужу книгу.");
            }

            return bookData.BookData;
        }
    }
}