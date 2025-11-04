using MediatR;
//using LNUBookShareBLL.Dtos;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Books
{
    public class GetBookDetailsQuery : IRequest<BookDetailsDto>
    {
        /// <summary>
        /// ID книги, яку ми хочемо завантажити
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// ID користувача, який дивиться сторінку (для кнопки "Вподобати")
        /// </summary>
        public int CurrentUserId { get; set; }
    }
}