using System.Text.RegularExpressions;

using LNUBookShareBLL.Common;
using LNUBookShareBLL.DTOs;

using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;


namespace LNUBookShareBLL.Features.Profile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public UpdateProfileCommandHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            this.ValidateRequest(request.Dto);

            var user = await this.GetUserAsync(request.UserId, cancellationToken);

            var avatarId = await this.GetAvatarIdAsync(request.Dto.ProfileImageUrl, cancellationToken);

            this.UpdateUserEntity(user, request.Dto, avatarId);

            await this._dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        private void ValidateRequest(ProfileEditDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName) || !Regex.IsMatch(dto.FirstName, @"^[a-zA-Zа-яА-ЯіІїЇєЄ']+$"))
            {
                throw new Exception("Ім'я повинно містити лише літери.");
            }

            if (string.IsNullOrWhiteSpace(dto.LastName) || !Regex.IsMatch(dto.LastName, @"^[a-zA-Zа-яА-ЯіІїЇєЄ']+$"))
            {
                throw new Exception("Прізвище повинно містити лише літери.");
            }

            if (dto.FacultyId <= 0)
            {
                throw new Exception("Необхідно обрати факультет.");
            }
        }

        private async Task<User> GetUserAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await this._dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            if (user == null)
            {
                throw new Exception("Користувача не знайдено.");
            }

            return user;
        }

        private async Task<int?> GetAvatarIdAsync(string imagePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return null;
            }

            string relativePath = PathHelper.ConvertToRelativePath(imagePath);

            var image = await this._dbContext.Images
                .FirstOrDefaultAsync(img => img.ImagePath == relativePath, cancellationToken);

            if (image == null)
            {
                Console.WriteLine($"Увага: не вдалося знайти Image за шляхом {relativePath}");
                return null;
            }

            return image.ImageId;
        }

        private void UpdateUserEntity(User user, ProfileEditDto dto, int? avatarId)
        {
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.FacultyId = dto.FacultyId;
            user.UpdatedAt = DateTime.UtcNow;

            if (avatarId.HasValue)
            {
                user.AvatarId = avatarId.Value;
            }
        }
    }
}