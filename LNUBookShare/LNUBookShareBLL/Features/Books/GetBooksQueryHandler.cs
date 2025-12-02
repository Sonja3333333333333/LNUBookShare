using LNUBookShareBLL.Common;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;

using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareBLL.Features.Books
{
    public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, PaginatedResultDto<BookCardDto>>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetBooksQueryHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<PaginatedResultDto<BookCardDto>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
        {
            var query = this._dbContext.Books
                .AsNoTracking()
                .Include(book => book.Owner)
                .Include(book => book.Cover)
                .Include(book => book.Category)
                .AsQueryable();

            if (request.RecommendForUser)
            {
                var currentUserFacultyId = await _dbContext.Users
                    .Where(u => u.UserId == request.CurrentUserId)
                    .Select(u => u.FacultyId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (currentUserFacultyId > 0)
                {
                    query = query.Where(b => b.Owner.FacultyId == currentUserFacultyId);

                    query = query.Where(b => b.OwnerId != request.CurrentUserId);
                }
            }

            query = this.ApplyFilters(query, request);

            var totalCount = await query.CountAsync(cancellationToken);

            query = this.ApplySorting(query, request.SortBy);

            var paginatedQuery = this.ApplyPagination(query, request.PageNumber, request.PageSize);

            var books = await this.ProjectToDtoAsync(paginatedQuery, request.CurrentUserId, cancellationToken);

            return new PaginatedResultDto<BookCardDto>
            {
                Items = books,
                TotalCount = totalCount
            };
        }

        private IQueryable<Book> ApplyFilters(IQueryable<Book> query, GetBooksQuery request)
        {
            switch (request.FilterBy)
            {
                case BookFilterStatus.Available:
                    query = query.Where(book => book.Status == "available");
                    break;
                case BookFilterStatus.Issued:
                    query = query.Where(book => book.Status == "issued");
                    break;
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                switch (request.SearchBy)
                {
                    case BookSearchCriteria.Title:
                        query = query.Where(book => book.Title.ToLower().Contains(term));
                        break;
                    case BookSearchCriteria.Author:
                        query = query.Where(book => book.Author.ToLower().Contains(term));
                        break;
                    case BookSearchCriteria.ISBN:
                        query = query.Where(book => book.Isbn != null && book.Isbn.ToLower().Contains(term));
                        break;
                    case BookSearchCriteria.Category:
                        query = query.Where(book => book.Category != null && book.Category.Name.ToLower().Contains(term));
                        break;
                }
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

        private async Task<List<BookCardDto>> ProjectToDtoAsync(IQueryable<Book> query, int currentUserId, CancellationToken cancellationToken)
        {
            return await query
                .Select(book => new BookCardDto
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    Year = book.Year,
                    Status = book.Status,
                    CoverPath = PathHelper.ConvertToAbsolutePath(book.Cover != null ? book.Cover.ImagePath : null),
                    OwnerFullName = (book.Owner != null) ? (book.Owner.FirstName + " " + book.Owner.LastName) : "Власник невідомий",
                    OwnerId = (book.Owner != null) ? book.Owner.UserId : 0,
                    IsFavoritedByCurrentUser = this._dbContext.Favorites.Any(favorite =>
                        favorite.BookId == book.BookId && favorite.UserId == currentUserId)
                })
                .ToListAsync(cancellationToken);
        }
    }
}