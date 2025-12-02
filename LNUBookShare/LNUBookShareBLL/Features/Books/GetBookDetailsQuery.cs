using LNUBookShareBLL.DTOs;

using MediatR;

namespace LNUBookShareBLL.Features.Books
{
    public class GetBookDetailsQuery : IRequest<BookDetailsDto>
    {
        public int BookId { get; set; }

        public int CurrentUserId { get; set; }
    }
}