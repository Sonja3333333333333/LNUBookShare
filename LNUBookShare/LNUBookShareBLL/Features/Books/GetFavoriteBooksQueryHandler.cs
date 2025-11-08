using MediatR;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;
using Microsoft.EntityFrameworkCore;

using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Favorites
{
    public class GetFavoriteBooksQueryHandler : IRequestHandler<GetFavoriteBooksQuery, PaginatedResultDto<FavoriteBookCardDto>>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetFavoriteBooksQueryHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaginatedResultDto<FavoriteBookCardDto>> Handle(GetFavoriteBooksQuery request, CancellationToken cancellationToken)
        {
            // 1. Починаємо запит з таблиці "Favorites"
            var query = _dbContext.Favorites
                // Фільтруємо за поточним користувачем
                .Where(f => f.UserId == request.CurrentUserId)
                // Включаємо пов'язані дані *перед* проекцією
                .Include(f => f.Book.Owner)
                .Include(f => f.Book.Cover)
                // "Розгортаємо" запит, щоб працювати зі списком Книг (Book)
                .Select(f => f.Book);

            // 2. Застосовуємо ФІЛЬТРАЦІЮ (WHERE) за статусом
            switch (request.FilterBy)
            {
                case BookFilterStatus.Available:
                    query = query.Where(b => b.Status == "available");
                    break;
                case BookFilterStatus.Issued:
                    query = query.Where(b => b.Status == "issued");
                    break;
            }

            // 3. Отримуємо ЗАГАЛЬНУ КІЛЬКІСТЬ (до пагінації!)
            var totalCount = await query.CountAsync(cancellationToken);

            // 4. Застосовуємо СОРТУВАННЯ (ORDER BY)
            switch (request.SortBy)
            {
                case BookSortCriteria.Author:
                    query = query.OrderBy(b => b.Author);
                    break;
                case BookSortCriteria.Year:
                    query = query.OrderBy(b => b.Year);
                    break;
                default:
                    query = query.OrderBy(b => b.Title); // Сортування за Назвою за замовчуванням
                    break;
            }

            // 5. Застосовуємо ПАГІНАЦІЮ (SKIP / TAKE)
            var paginatedQuery = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            // 6. Проекція (SELECT) - Виконуємо запит до БД
            var books = await paginatedQuery
                .Select(book => new FavoriteBookCardDto
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    Year = book.Year,
                    Status = book.Status,
                    CoverPath = (book.Cover != null) ? book.Cover.ImagePath : null,
                    OwnerFullName = (book.Owner != null) ? (book.Owner.FirstName + " " + book.Owner.LastName) : "N/A",
                    //OwnerEmail = (book.Owner != null) ? book.Owner.Email : "N/A"
                })
                .ToListAsync(cancellationToken);

            // 7. Повертаємо фінальний DTO
            return new PaginatedResultDto<FavoriteBookCardDto>
            {
                Items = books,
                TotalCount = totalCount
            };
        }
    }
}
