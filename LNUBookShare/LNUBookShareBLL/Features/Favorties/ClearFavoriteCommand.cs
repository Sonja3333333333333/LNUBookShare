using MediatR;

namespace LNUBookShareBLL.Features.Favorites
{
    /// <summary>
    /// Команда для повного очищення списку вподобань для
    /// конкретного користувача.
    /// </summary>
    public class ClearFavoritesCommand : IRequest
    {
        // Нам потрібен ID, щоб знати, чиї вподобання чистити
        public int UserId { get; set; }
    }
}