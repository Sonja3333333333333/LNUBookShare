using LNUBookShareBLL.DTOs;

using MediatR;

namespace LNUBookShareBLL.Features.Books
{
    public class UpdateBookCommand : IRequest
    {
        public int BookId { get; set; }

        public int CurrentUserId { get; set; }

        public BookEditDto? Dto { get; set; }
    }
}