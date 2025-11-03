using MediatR;

namespace LNUBookShareBLL.Features.Favorites
{
    public class ToggleFavoriteCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
    }
}