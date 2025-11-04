using MediatR;

namespace LNUBookShareBLL.Features.Books
{
    public class DeleteBookCommand : IRequest
    {
        public int BookId { get; set; }

        /// <summary>
        /// ID користувача, який намагається видалити.
        /// (Для перевірки, чи він є власником)
        /// </summary>
        public int CurrentUserId { get; set; }
    }
}
