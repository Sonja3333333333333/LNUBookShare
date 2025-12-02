using LNUBookShareBLL.DTOs;

using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;

using static BCrypt.Net.BCrypt;

namespace LNUBookShareBLL.Features.Auth
{
    public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, LoginResultDto>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public LoginUserQueryHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<LoginResultDto> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {
            this.ValidateRequest(request);

            var user = await this.GetUserByEmailAsync(request.Email, cancellationToken);

            this.ValidateUserCredentials(user, request.Password);

            return this.MapUserToDto(user);
        }

        private void ValidateRequest(LoginUserQuery request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.EndsWith("@lnu.edu.ua"))
            {
                throw new Exception("Введіть email @lnu.edu.ua");
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 9)
            {
                throw new Exception("Пароль >= 9 символів");
            }
        }

        private async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await this._dbContext.Users
                .Include(user => user.Faculty)
                .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
        }

        private void ValidateUserCredentials(User user, string providedPassword)
        {
            if (user == null)
            {
                throw new Exception("Невірний email або пароль.");
            }

            var isPasswordValid = Verify(providedPassword, user.PasswordHash);

            if (!isPasswordValid)
            {
                throw new Exception("Невірний email або пароль.");
            }

            if (!user.IsEmailConfirmed)
            {
                throw new Exception("Ваш акаунт не підтверджено. Будь ласка, перевірте пошту.");
            }
        }

        private LoginResultDto MapUserToDto(User user)
        {
            return new LoginResultDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                FacultyName = user.Faculty?.Name ?? "Не вказано"
            };
        }
    }
}