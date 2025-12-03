using LNUBookShareBLL.DTOs;

using MediatR;

namespace LNUBookShareBLL.Features.Books
{
    public class AddBookCommand : IRequest<int>
    {
        public int OwnerUserId { get; set; }

        public AddBookDto? Dto { get; set; }
    }
}