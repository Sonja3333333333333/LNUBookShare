using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Profile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public UpdateProfileCommandHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            // --- Валідація (та сама, що й при реєстрації) ---
            if (string.IsNullOrWhiteSpace(request.Dto.FirstName) || !Regex.IsMatch(request.Dto.FirstName, @"^[a-zA-Zа-яА-ЯіІїЇєЄ']+$"))
            {
                throw new Exception("Ім'я повинно містити лише літери.");
            }
            if (string.IsNullOrWhiteSpace(request.Dto.LastName) || !Regex.IsMatch(request.Dto.LastName, @"^[a-zA-Zа-яА-ЯіІїЇєЄ']+$"))
            {
                throw new Exception("Прізвище повинно містити лише літери.");
            }
            if (request.Dto.FacultyId <= 0)
            {
                throw new Exception("Необхідно обрати факультет.");
            }

            // --- Оновлення ---
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new Exception("Користувача не знайдено.");
            }

            // Оновлюємо поля
            user.FirstName = request.Dto.FirstName;
            user.LastName = request.Dto.LastName;
            user.FacultyId = request.Dto.FacultyId;
            user.UpdatedAt = DateTime.UtcNow; // Не забуваємо оновити дату

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}