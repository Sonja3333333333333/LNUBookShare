using MediatR;
using LNUBookShareBLL.Dtos;
using LNUBookShareBLL.Enums;
using Microsoft.EntityFrameworkCore;

using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Books
{
    public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, PaginatedResultDto<BookCardDto>>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetBooksQueryHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaginatedResultDto<BookCardDto>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
        {
            // 1. Починаємо будувати запит (ще не виконуємо)
            var query = _dbContext.Books
                .Include(b => b.Owner)    // Потрібен для імені власника
                .Include(b => b.Cover)    // Для обкладинки
                .Include(b => b.Category) // Для пошуку/сортування
                .AsQueryable();

            // 2. Застосовуємо ФІЛЬТРАЦІЮ (WHERE)

            // Фільтр статусу ("Усе", "Тільки доступні", "Тільки видані")
            switch (request.FilterBy)
            {
                case BookFilterStatus.Available:
                    query = query.Where(b => b.Status == "available");
                    break;
                case BookFilterStatus.Issued:
                    query = query.Where(b => b.Status == "issued");
                    break;
            }

            // Пошуковий запит (якщо він є)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                switch (request.SearchBy)
                {
                    case BookSearchCriteria.Title:
                        query = query.Where(b => b.Title.ToLower().Contains(term));
                        break;
                    case BookSearchCriteria.Author:
                        query = query.Where(b => b.Author.ToLower().Contains(term));
                        break;
                    case BookSearchCriteria.ISBN:
                        query = query.Where(b => b.Isbn != null && b.Isbn.ToLower().Contains(term));
                        break;
                    case BookSearchCriteria.Category:
                        query = query.Where(b => b.Category.Name.ToLower().Contains(term));
                        break;
                }
            }

            // 3. Отримуємо ЗАГАЛЬНУ КІЛЬКІСТЬ (до пагінації!)
            var totalCount = await query.CountAsync(cancellationToken);

            // 4. Застосовуємо СОРТУВАННЯ (ORDER BY)
            switch (request.SortBy)
            {
                case BookSortCriteria.Author:
                    query = query.OrderBy(b => b.Author);
                    break;
                case BookSortCriteria.Category:
                    query = query.OrderBy(b => b.Category.Name);
                    break;
                case BookSortCriteria.Language:
                    query = query.OrderBy(b => b.Language);
                    break;
                case BookSortCriteria.Year:
                    query = query.OrderBy(b => b.Year);
                    break;
                default:
                    query = query.OrderBy(b => b.Title); // За замовчуванням
                    break;
            }

            // 5. Застосовуємо ПАГІНАЦІЮ (SKIP / TAKE)
            var paginatedQuery = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            // 6. Проекція (SELECT) - Виконуємо запит до БД
            // Перетворюємо Book (DAL) -> BookCardDto (BLL)
            var books = await paginatedQuery
                .Select(book => new BookCardDto
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    Year = book.Year,
                    CoverPath = book.Cover != null ? book.Cover.ImagePath : null,
                    OwnerFullName = book.Owner.FirstName + " " + book.Owner.LastName,

                    // Суб-запит: перевіряємо, чи є запис у таблиці Favorites
                    IsFavoritedByCurrentUser = _dbContext.Favorites.Any(f =>
                        f.BookId == book.BookId && f.UserId == request.CurrentUserId)
                })
                .ToListAsync(cancellationToken);

            // 7. Повертаємо фінальний DTO
            return new PaginatedResultDto<BookCardDto>
            {
                Items = books,
                TotalCount = totalCount
            };
        }
    }
}