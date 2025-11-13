using MediatR;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Books
{
    /// <summary>
    /// Команда для створення нової книги. Повертає ID створеної книги.
    /// </summary>
    public class AddBookCommand : IRequest<int>
    {
        public int OwnerUserId { get; set; }
        public AddBookDto? Dto { get; set; }
    }
}