using MediatR;
using LNUBookShareBLL.DTOs;
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
            var query = _dbContext.Books
                .Include(b => b.Owner)
                .Include(b => b.Cover)
                .Include(b => b.Category)
                .AsQueryable();

            // 2. ФІЛЬТРАЦІЯ
            switch (request.FilterBy)
            {
                case BookFilterStatus.Available:
                    query = query.Where(b => b.Status == "available");
                    break;
                case BookFilterStatus.Issued:
                    query = query.Where(b => b.Status == "issued");
                    break;
            }

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
                        query = query.Where(b => b.Category != null && b.Category.Name.ToLower().Contains(term));
                        break;
                }
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // 4. СОРТУВАННЯ
            switch (request.SortBy)
            {
                case BookSortCriteria.Author:
                    query = query.OrderBy(b => b.Author);
                    break;
                case BookSortCriteria.Year:
                    query = query.OrderBy(b => b.Year);
                    break;
                default:
                    query = query.OrderBy(b => b.Title);
                    break;
            }

            // 5. ПАГІНАЦІЯ
            var paginatedQuery = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            // 6. ПРОЕКЦІЯ (SELECT)
            var books = await paginatedQuery
                .Select(book => new BookCardDto
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    Year = book.Year,
                    Status = book.Status,
                    CoverPath = (book.Cover != null) ? book.Cover.ImagePath : null,
                    OwnerFullName = (book.Owner != null) ? (book.Owner.FirstName + " " + book.Owner.LastName) : "Власник невідомий",
                    OwnerId = (book.Owner != null) ? book.Owner.UserId : 0,    //додано OwnerId
                    IsFavoritedByCurrentUser = _dbContext.Favorites.Any(f =>
                        f.BookId == book.BookId && f.UserId == request.CurrentUserId)
                })
                .ToListAsync(cancellationToken);

            // 7. РЕЗУЛЬТАТ
            return new PaginatedResultDto<BookCardDto>
            {
                Items = books,
                TotalCount = totalCount
            };
        }
    }
}