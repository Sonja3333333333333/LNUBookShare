using MediatR;
using LNUBookShareDAL;
using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Favorites
{
    public class ClearFavoritesCommandHandler : IRequestHandler<ClearFavoritesCommand>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public ClearFavoritesCommandHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(ClearFavoritesCommand request, CancellationToken cancellationToken)
        {
            // 1. Знаходимо ВСІ записи вподобань для цього користувача
            var favoritesToRemove = await _dbContext.Favorites
                .Where(f => f.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            if (favoritesToRemove.Any())
            {
                // 2. Видаляємо їх
                _dbContext.Favorites.RemoveRange(favoritesToRemove);

                // 3. Зберігаємо зміни
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value; // Повертаємо "успіх" (void)
        }
    }
}