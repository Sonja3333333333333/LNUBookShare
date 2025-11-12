using MediatR;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Books
{
    public class UpdateBookCommand : IRequest
    {
        public int BookId { get; set; }
        public int CurrentUserId { get; set; } // Для перевірки прав
        public BookEditDto Dto { get; set; }
    }
}