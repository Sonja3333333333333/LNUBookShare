using LNUBookShareBLL.Common;
using LNUBookShareBLL.DTOs;

using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;


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
            var user = await this.GetUserAsync(request.UserId, cancellationToken);
            var ownedBooks = await this.GetOwnedBooksAsync(request.UserId, cancellationToken);
            var profileDto = this.MapToProfileDto(user, ownedBooks);

            return profileDto;
        }

        private async Task<User> GetUserAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await this._dbContext.Users
                .AsNoTracking()
                .Include(user => user.Faculty)
                .Include(user => user.Avatar)
                .FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);

            if (user == null)
            {
                throw new System.Exception("Користувача не знайдено.");
            }

            return user;
        }

        private async Task<List<OwnedBookDto>> GetOwnedBooksAsync(int userId, CancellationToken cancellationToken)
        {
            return await this._dbContext.Books
                .AsNoTracking()
                .Include(book => book.Cover)
                .Where(book => book.OwnerId == userId)
                .Select(book => new OwnedBookDto
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    Year = book.Year,
                    Status = book.Status,
                    CoverPath = PathHelper.ConvertToAbsolutePath(book.Cover != null ? book.Cover.ImagePath : null)
                })
                .ToListAsync(cancellationToken);
        }

        private ProfileDto MapToProfileDto(User user, List<OwnedBookDto> ownedBooks)
        {
            string finalAvatarPath = PathHelper.ConvertToAbsolutePath(user.Avatar?.ImagePath);

            return new ProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                FacultyName = user.Faculty?.Name ?? "Не вказано",
                AvatarPath = finalAvatarPath,
                OwnedBooks = ownedBooks
            };
        }
    }
}