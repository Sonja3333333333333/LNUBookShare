using MediatR;
using Microsoft.EntityFrameworkCore;
using LNUBookShareBLL.DTOs;
using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Profile
{
    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ProfileDto>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetProfileQueryHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<ProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            // 1. Шукаємо користувача і одразу підтягуємо його факультет та аватар
            var user = await this._dbContext.Users
                .Include(u => u.Faculty)
                .Include(u => u.Avatar)
                .AsNoTracking() // Це запит "тільки для читання"
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new System.Exception("Користувача не знайдено.");
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string? finalAvatarPath = null;
            if (user.Avatar != null && !string.IsNullOrEmpty(user.Avatar.ImagePath))
            {
                string dbPath = user.Avatar.ImagePath;
                // Перевіряємо, чи це URL
                if (dbPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    dbPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    finalAvatarPath = dbPath; // Це URL, використовуємо як є
                }
                else
                {
                    finalAvatarPath = Path.Combine(baseDir, dbPath); // Це файл, робимо абсолютним
                }
            }

            var ownedBooks = await this._dbContext.Books
                .Include(b => b.Cover)
                .Where(b => b.OwnerId == request.UserId)
                .AsNoTracking()
                .Select(b => new OwnedBookDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Author = b.Author,
                    Year = b.Year,
                    Status = b.Status,
                    // Застосовуємо ту саму логіку до обкладинок:
                    CoverPath = (b.Cover == null || string.IsNullOrEmpty(b.Cover.ImagePath))
                        ? null // Немає шляху
                        : (b.Cover.ImagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || b.Cover.ImagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            ? b.Cover.ImagePath // Це URL
                            : Path.Combine(baseDir, b.Cover.ImagePath) // Це файл
                })
                .ToListAsync(cancellationToken);

            var profileDto = new ProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                FacultyName = user.Faculty?.Name ?? "Не вказано",
                AvatarPath = finalAvatarPath, 
                OwnedBooks = ownedBooks
            };

            

            return profileDto;
        }
    }
}
