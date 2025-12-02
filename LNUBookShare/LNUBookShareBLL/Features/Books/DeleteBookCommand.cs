using MediatR;

namespace LNUBookShareBLL.Features.Books
{
    public class DeleteBookCommand : IRequest
    {
        public int BookId { get; set; }


        public int CurrentUserId { get; set; }
    }
}
