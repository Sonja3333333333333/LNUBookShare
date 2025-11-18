using MediatR;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Books
{
    public class GetBookDetailsQuery : IRequest<BookDetailsDto>
    {
        
        public int BookId { get; set; }

     
        public int CurrentUserId { get; set; }
    }
}