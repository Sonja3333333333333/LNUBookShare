using MediatR;

namespace LNUBookShareBLL.Features.Favorites
{
    public class ClearFavoritesCommand : IRequest
    {
        public int UserId { get; set; }
    }
}