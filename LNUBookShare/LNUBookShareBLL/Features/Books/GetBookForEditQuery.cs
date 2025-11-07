using MediatR;
//using LNUBookShareBLL.Dtos;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Books
{
    /// <summary>
    /// Запит на отримання поточних даних книги для редагування.
    /// </summary>
    public class GetBookForEditQuery : IRequest<BookEditDto>
    {
        public int BookId { get; set; }
        public int CurrentUserId { get; set; } // Для перевірки, чи це ВЛАСНИК
    }
}