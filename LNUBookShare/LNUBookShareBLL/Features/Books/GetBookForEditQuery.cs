using LNUBookShareBLL.DTOs;

using MediatR;

namespace LNUBookShareBLL.Features.Books
{
    public class GetBookForEditQuery : IRequest<BookEditDto>
    {
        public int BookId { get; set; }
        public int CurrentUserId { get; set; }
    }
}