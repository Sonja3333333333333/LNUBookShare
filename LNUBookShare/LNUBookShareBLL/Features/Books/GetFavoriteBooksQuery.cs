using MediatR;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;

namespace LNUBookShareBLL.Features.Favorites
{
    public class GetFavoriteBooksQuery : IRequest<PaginatedResultDto<FavoriteBookCardDto>>
    {
        public int CurrentUserId { get; set; }

        public BookFilterStatus FilterBy { get; set; } = BookFilterStatus.All;
        public BookSortCriteria SortBy { get; set; } = BookSortCriteria.Title;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}