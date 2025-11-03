using MediatR;
using LNUBookShareBLL.Dtos;
using LNUBookShareBLL.Enums;

namespace LNUBookShareBLL.Features.Books
{
    public class GetBooksQuery : IRequest<PaginatedResultDto<BookCardDto>>
    {
        public int CurrentUserId { get; set; }
        public string? SearchTerm { get; set; }
        public BookSearchCriteria SearchBy { get; set; } = BookSearchCriteria.Title;
        public BookFilterStatus FilterBy { get; set; } = BookFilterStatus.All;
        public BookSortCriteria SortBy { get; set; } = BookSortCriteria.Title;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}