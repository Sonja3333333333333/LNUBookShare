using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;

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
            var favoriteEntry = await this.GetExistingFavoriteAsync(request, cancellationToken);

            if (favoriteEntry != null)
            {
                await this.RemoveFavoriteAsync(favoriteEntry, cancellationToken);
                return false;
            }
            else
            {
                await this.AddNewFavoriteAsync(request, cancellationToken);
                return true;
            }
        }

        private async Task<Favorite> GetExistingFavoriteAsync(ToggleFavoriteCommand request, CancellationToken cancellationToken)
        {
            return await this._dbContext.Favorites
                .FirstOrDefaultAsync(
                    favorite =>
                    favorite.UserId == request.UserId && favorite.BookId == request.BookId,
                    cancellationToken);
        }

        private async Task RemoveFavoriteAsync(Favorite favoriteEntry, CancellationToken cancellationToken)
        {
            this._dbContext.Favorites.Remove(favoriteEntry);
            await this._dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task AddNewFavoriteAsync(ToggleFavoriteCommand request, CancellationToken cancellationToken)
        {
            await this.ValidateBookExistsAsync(request.BookId, cancellationToken);

            var newFavorite = new Favorite
            {
                UserId = request.UserId,
                BookId = request.BookId,
                CreatedAt = System.DateTime.UtcNow,
            };

            await this._dbContext.Favorites.AddAsync(newFavorite, cancellationToken);
            await this._dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task ValidateBookExistsAsync(int bookId, CancellationToken cancellationToken)
        {
            var bookExists = await this._dbContext.Books.AnyAsync(book => book.BookId == bookId, cancellationToken);
            if (!bookExists)
            {
                throw new System.Exception("Книгу не знайдено.");
            }
        }
    }
}