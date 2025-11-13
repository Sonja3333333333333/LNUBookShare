using MediatR;
using LNUBookShareDAL;
using LNUBookShareDAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace LNUBookShareBLL.Features.Favorites
{
    public class ToggleFavoriteCommandHandler : IRequestHandler<ToggleFavoriteCommand, bool>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public ToggleFavoriteCommandHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<bool> Handle(ToggleFavoriteCommand request, CancellationToken cancellationToken)
        {
            var favoriteEntry = await this._dbContext.Favorites
                .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.BookId == request.BookId, cancellationToken);

            if (favoriteEntry != null)
            {
                _ = this._dbContext.Favorites.Remove(favoriteEntry);
                _ = await this._dbContext.SaveChangesAsync(cancellationToken);
                return false;
            }
            else
            {
                var bookExists = await this._dbContext.Books.AnyAsync(b => b.BookId == request.BookId, cancellationToken);
                if (!bookExists)
                {
                    throw new System.Exception("Книгу не знайдено.");
                }

                var newFavorite = new Favorite
                {
                    UserId = request.UserId,
                    BookId = request.BookId,
                    CreatedAt = System.DateTime.UtcNow
                };

                _ = await this._dbContext.Favorites.AddAsync(newFavorite, cancellationToken);
                _ = await this._dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
        }
    }
}