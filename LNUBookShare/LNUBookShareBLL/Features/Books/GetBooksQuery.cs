using MediatR;
using LNUBookShareBLL.Dtos;
using LNUBookShareBLL.Enums;

namespace LNUBookShareBLL.Features.Books
{
    public class GetBooksQuery : IRequest<PaginatedResultDto<BookCardDto>>
    {
        /// <summary>
        /// ID користувача, який дивиться каталог (важливо для "сердечок").
        /// </summary>
        public int CurrentUserId { get; set; }

        // --- Пошук ---
        public string? SearchTerm { get; set; }
        public BookSearchCriteria SearchBy { get; set; } = BookSearchCriteria.Title;

        // --- Фільтрація ---
        public BookFilterStatus FilterBy { get; set; } = BookFilterStatus.All;

        // --- Сортування ---
        public BookSortCriteria SortBy { get; set; } = BookSortCriteria.Author;

        // --- Пагінація ---
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10; // Кількість книг на сторінці
    }
}