using MediatR;

namespace LNUBookShareBLL.Features.Favorites
{
    /// <summary>
    /// Команда для додавання/видалення книги з уподобань.
    /// Повертає 'true', якщо книга стала вподобаною, і 'false', якщо ні.
    /// </summary>
    public class ToggleFavoriteCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
    }
}