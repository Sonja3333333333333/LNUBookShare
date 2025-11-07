using MediatR;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Books
{
    /// <summary>
    /// Команда для збереження оновлених даних книги.
    /// </summary>
    public class UpdateBookCommand : IRequest
    {
        public int BookId { get; set; }
        public int CurrentUserId { get; set; } // Для перевірки, чи це ВЛАСНИК
        public BookEditDto Dto { get; set; }
    }
}