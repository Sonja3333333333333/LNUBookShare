using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;


namespace LNUBookShareBLL.Features.Favorites
{
    public class ClearFavoritesCommandHandler : IRequestHandler<ClearFavoritesCommand>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public ClearFavoritesCommandHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Unit> Handle(ClearFavoritesCommand request, CancellationToken cancellationToken)
        {
            var favoritesToRemove = await this.GetFavoritesForUserAsync(request.UserId, cancellationToken);

            if (favoritesToRemove.Any())
            {
                await this.DeleteFavoritesAsync(favoritesToRemove, cancellationToken);
            }

            return Unit.Value;
        }

        private async Task<List<Favorite>> GetFavoritesForUserAsync(int userId, CancellationToken cancellationToken)
        {
            return await this._dbContext.Favorites
                .Where(favorite => favorite.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        private async Task DeleteFavoritesAsync(List<Favorite> favoritesToRemove, CancellationToken cancellationToken)
        {
            this._dbContext.Favorites.RemoveRange(favoritesToRemove);
            await this._dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}