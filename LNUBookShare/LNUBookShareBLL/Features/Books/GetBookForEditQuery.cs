using MediatR;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Books
{
 
    public class GetBookForEditQuery : IRequest<BookEditDto>
    {
        public int BookId { get; set; }
        public int CurrentUserId { get; set; } 
    }
}