using MediatR;
using LNUBookShareDAL.Models;
using Microsoft.EntityFrameworkCore;

//Обробник команди "Уподобати". Вона повертатиме bool (новий стан: true = тепер вподобано, false = тепер не вподобано), щоб UI міг миттєво оновити сердечко.

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
            // 1. Шукаємо, чи вже існує такий запис
            var favoriteEntry = await _dbContext.Favorites
                .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.BookId == request.BookId, cancellationToken);

            if (favoriteEntry != null)
            {
                // 2. Вже вподобано -> ВИДАЛЯЄМО
                _dbContext.Favorites.Remove(favoriteEntry);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return false; // Новий стан: не вподобано
            }
            else
            {
                // 3. Ще не вподобано -> ДОДАЄМО

                // Перевірка, чи існує книга (щоб уникнути помилок)
                var bookExists = await _dbContext.Books.AnyAsync(b => b.BookId == request.BookId, cancellationToken);
                if (!bookExists)
                {
                    throw new Exception("Книгу не знайдено.");
                }

                var newFavorite = new Favorite
                {
                    UserId = request.UserId,
                    BookId = request.BookId,
                    CreatedAt = System.DateTime.UtcNow
                };

                await _dbContext.Favorites.AddAsync(newFavorite, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true; // Новий стан: вподобано
            }
        }
    }
}