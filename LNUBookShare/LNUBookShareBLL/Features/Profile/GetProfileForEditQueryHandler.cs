using LNUBookShareBLL.DTOs;
using LNUBookShareDAL; 
using LNUBookShareDAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LNUBookShareBLL.Features.Profile
{
    public class GetProfileForEditQueryHandler : IRequestHandler<GetProfileForEditQuery, ProfileEditDto>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetProfileForEditQueryHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProfileEditDto> Handle(GetProfileForEditQuery request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .Include(u => u.Avatar) 
                .Include(u => u.Faculty) 
                .Where(u => u.UserId == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                throw new Exception("Користувача не знайдено.");
            }

            var profileDto = new ProfileEditDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                FacultyId = user.FacultyId,
                ProfileImageUrl = user.Avatar?.ImagePath
            };

            return profileDto;
        }
    }
}