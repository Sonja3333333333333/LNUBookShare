using MediatR;
using LNUBookShareBLL.Dtos;
using Microsoft.EntityFrameworkCore; // Для .Include() та .FirstOrDefaultAsync()
using static BCrypt.Net.BCrypt; // Для методу Verify()

using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Auth
{
    public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, LoginResultDto>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public LoginUserQueryHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LoginResultDto> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.EndsWith("@lnu.edu.ua"))
            {
                throw new Exception("Введіть email @lnu.edu.ua");
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 9)
            {
                throw new Exception("Пароль >= 9 символів");
            }


            var user = await _dbContext.Users
                .Include(u => u.Faculty) 
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null)
            {
                throw new Exception("Невірний email або пароль.");
            }

            var isPasswordValid = Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                throw new Exception("Невірний email або пароль.");
            }

            if (!user.IsEmailConfirmed)
            {
                throw new Exception("Ваш акаунт не підтверджено. Будь ласка, перевірте пошту.");
            }

            var resultDto = new LoginResultDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                FacultyName = user.Faculty?.Name ?? "Не вказано" 
            };

            return resultDto;
        }
    }
}