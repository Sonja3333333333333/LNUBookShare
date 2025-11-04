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
            _dbContext = dbContext;
        }

        public async Task<ProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            // 1. Шукаємо користувача і одразу підтягуємо його факультет та аватар
            var user = await _dbContext.Users
                .Include(u => u.Faculty)
                .Include(u => u.Avatar)
                .AsNoTracking() // Це запит "тільки для читання"
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new System.Exception("Користувача не знайдено.");
            }

            // 2. Окремо завантажуємо список книг, що належать цьому користувачу
            var ownedBooks = await _dbContext.Books
                .Include(b => b.Cover) // Підтягуємо обкладинки
                .Where(b => b.OwnerId == request.UserId)
                .AsNoTracking()
                .Select(b => new OwnedBookDto // Перетворюємо на DTO
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Author = b.Author,
                    Year = b.Year,
                    Status = b.Status, // "available" або "issued"
                    CoverPath = b.Cover != null ? b.Cover.ImagePath : null
                })
                .ToListAsync(cancellationToken);

            // 3. Збираємо все у фінальний DTO
            var profileDto = new ProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                FacultyName = user.Faculty?.Name ?? "Не вказано",
                AvatarPath = user.Avatar?.ImagePath,
                OwnedBooks = ownedBooks
            };

            return profileDto;
        }
    }
}
