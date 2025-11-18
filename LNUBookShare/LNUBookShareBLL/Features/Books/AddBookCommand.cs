using MediatR;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Books
{
    
    public class AddBookCommand : IRequest<int>
    {
        public int OwnerUserId { get; set; }
        public AddBookDto? Dto { get; set; }
    }
}