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
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(ToggleFavoriteCommand request, CancellationToken cancellationToken)
        {
            var favoriteEntry = await _dbContext.Favorites
                .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.BookId == request.BookId, cancellationToken);

            if (favoriteEntry != null)
            {
                _dbContext.Favorites.Remove(favoriteEntry);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return false;
            }
            else
            {
                var bookExists = await _dbContext.Books.AnyAsync(b => b.BookId == request.BookId, cancellationToken);
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

                await _dbContext.Favorites.AddAsync(newFavorite, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
        }
    }
}