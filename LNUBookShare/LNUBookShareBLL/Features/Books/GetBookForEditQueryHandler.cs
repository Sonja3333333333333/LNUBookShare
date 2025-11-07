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
            _dbContext = dbContext;
        }

        public async Task<BookEditDto> Handle(GetBookForEditQuery request, CancellationToken cancellationToken)
        {
            var bookDto = await _dbContext.Books
                .AsNoTracking()
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
                        Status = b.Status
                    },
                    OwnerId = b.OwnerId // Потрібно для перевірки
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (bookDto == null)
            {
                throw new Exception("Книгу не знайдено.");
            }

            // ВАЖЛИВА ПЕРЕВІРКА: Редагувати книгу може тільки її власник
            if (bookDto.OwnerId != request.CurrentUserId)
            {
                throw new Exception("Ви не можете редагувати чужу книгу.");
            }

            return bookDto.BookData;
        }
    }
}