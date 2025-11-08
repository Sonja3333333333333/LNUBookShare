using MediatR;
using Microsoft.EntityFrameworkCore;
using LNUBookShareBLL.DTOs;
using LNUBookShareDAL.Models;

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
                .AsNoTracking() // Нам не потрібно відстежувати цей об'єкт, тільки читати
                .Where(u => u.UserId == request.UserId)
                .Select(u => new ProfileEditDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FacultyId = u.FacultyId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                throw new Exception("Користувача не знайдено.");
            }

            return user;
        }
    }
}