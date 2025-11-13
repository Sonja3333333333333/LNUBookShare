using LNUBookShareBLL.DTOs;
using LNUBookShareDAL; 
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
            var user = await this._dbContext.Users
                .AsNoTracking()
                .Include(u => u.Avatar) 
                .Include(u => u.Faculty) 
                .Where(u => u.UserId == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                throw new Exception("Користувача не знайдено.");
            }

            string? finalImagePath = null;
            if (user.Avatar != null && !string.IsNullOrEmpty(user.Avatar.ImagePath))
            {
                string dbPath = user.Avatar.ImagePath;
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                if (dbPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    dbPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    finalImagePath = dbPath; // Це URL
                }
                else
                {
                    finalImagePath = Path.Combine(baseDir, dbPath); // Це файл
                }
            }

            var profileDto = new ProfileEditDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                FacultyId = user.FacultyId,
                ProfileImageUrl = finalImagePath
            };

            return profileDto;
        }
    }
}