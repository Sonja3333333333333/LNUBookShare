using LNUBookShareBLL.Common;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;

using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareBLL.Features.Favorites
{
    public class GetFavoriteBooksQueryHandler : IRequestHandler<GetFavoriteBooksQuery, PaginatedResultDto<FavoriteBookCardDto>>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetFavoriteBooksQueryHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<PaginatedResultDto<FavoriteBookCardDto>> Handle(GetFavoriteBooksQuery request, CancellationToken cancellationToken)
        {
            var query = this.GetBaseQuery(request.CurrentUserId);

            query = this.ApplyFilters(query, request.FilterBy);

            var totalCount = await query.CountAsync(cancellationToken);

            query = this.ApplySorting(query, request.SortBy);

            var paginatedQuery = this.ApplyPagination(query, request.PageNumber, request.PageSize);

            var books = await this.ProjectToDtoAsync(paginatedQuery, cancellationToken);

            return new PaginatedResultDto<FavoriteBookCardDto>
            {
                Items = books,
                TotalCount = totalCount,
            };
        }

        private IQueryable<Book> GetBaseQuery(int currentUserId)
        {
            return this._dbContext.Favorites
                .AsNoTracking()
                .Where(favorite => favorite.UserId == currentUserId)
                .Include(favorite => favorite.Book.Owner)
                .Include(favorite => favorite.Book.Cover)
                .Select(favorite => favorite.Book);
        }

        private IQueryable<Book> ApplyFilters(IQueryable<Book> query, BookFilterStatus filterBy)
        {
            switch (filterBy)
            {
                case BookFilterStatus.Available:
                    query = query.Where(book => book.Status == "available");
                    break;
                case BookFilterStatus.Issued:
                    query = query.Where(book => book.Status == "issued");
                    break;
            }

            return query;
        }

        private IQueryable<Book> ApplySorting(IQueryable<Book> query, BookSortCriteria sortBy)
        {
            switch (sortBy)
            {
                case BookSortCriteria.Author:
                    return query.OrderBy(book => book.Author);
                case BookSortCriteria.Year:
                    return query.OrderBy(book => book.Year);
                default:
                    return query.OrderBy(book => book.Title);
            }
        }

        private IQueryable<Book> ApplyPagination(IQueryable<Book> query, int pageNumber, int pageSize)
        {
            return query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }

        private async Task<List<FavoriteBookCardDto>> ProjectToDtoAsync(IQueryable<Book> query, CancellationToken cancellationToken)
        {
            return await query
                .Select(book => new FavoriteBookCardDto
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    Year = book.Year,
                    Status = book.Status,
                    CoverPath = PathHelper.ConvertToAbsolutePath(book.Cover != null ? book.Cover.ImagePath : null),
                    OwnerFullName = (book.Owner != null) ? (book.Owner.FirstName + " " + book.Owner.LastName) : "N/A",
                    OwnerId = book.OwnerId,
                })
                .ToListAsync(cancellationToken);
        }
    }
}