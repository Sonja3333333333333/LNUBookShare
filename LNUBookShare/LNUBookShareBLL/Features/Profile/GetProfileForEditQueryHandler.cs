using LNUBookShareBLL.Common;
using LNUBookShareBLL.DTOs;

using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareBLL.Features.Profile
{
    public class GetProfileForEditQueryHandler : IRequestHandler<GetProfileForEditQuery, ProfileEditDto>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetProfileForEditQueryHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<ProfileEditDto> Handle(GetProfileForEditQuery request, CancellationToken cancellationToken)
        {
            var user = await this.GetUserAsync(request.UserId, cancellationToken);
            var profileDto = this.MapUserToDto(user);
            return profileDto;
        }

        private async Task<User> GetUserAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await this._dbContext.Users
                .AsNoTracking()
                .Include(user => user.Avatar)
                .Include(user => user.Faculty)
                .Where(user => user.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                throw new Exception("Користувача не знайдено.");
            }

            return user;
        }

        private ProfileEditDto MapUserToDto(User user)
        {
            var finalImagePath = PathHelper.ConvertToAbsolutePath(user.Avatar?.ImagePath);

            return new ProfileEditDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                FacultyId = user.FacultyId,
                ProfileImageUrl = finalImagePath,
            };
        }
    }
}