using System.Text.RegularExpressions;

using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;

using static BCrypt.Net.BCrypt;

namespace LNUBookShareBLL.Features.Auth
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, int>
    {
        private readonly LNUBookShareDbContext _dbContext;
        private readonly IEmailService _emailService;

        public RegisterUserCommandHandler(LNUBookShareDbContext dbContext, IEmailService emailService)
        {
            this._dbContext = dbContext;
            this._emailService = emailService;
        }

        public async Task<int> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            this.ValidateRequest(request);
            await this.CheckEmailUniquenessAsync(request.Email, cancellationToken);

            var (newUser, tokenEntity) = this.CreateUserAndTokenEntities(request);

            await this._dbContext.Users.AddAsync(newUser, cancellationToken);
            await this._dbContext.Emailconfirmations.AddAsync(tokenEntity, cancellationToken);

            string confirmationLink = $"https://localhost:7163/api/auth/confirm?token={tokenEntity.ConfirmationToken}";
            try
            {
                await this._dbContext.SaveChangesAsync(cancellationToken);

                await this._emailService.SendConfirmationEmailAsync(request.Email, confirmationLink);
            }
            catch (Exception ex)
            {
                _dbContext.Users.Remove(newUser);
                _dbContext.Emailconfirmations.Remove(tokenEntity);
                await this._dbContext.SaveChangesAsync();

                throw new Exception($"Не вдалося надіслати лист підтвердження: {ex.Message}");
            }

            return newUser.UserId;
        }

        private void ValidateRequest(RegisterUserCommand request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                throw new Exception("Ім'я не може бути порожнім.");
            }

            if (!Regex.IsMatch(request.FirstName, @"^[a-zA-Zа-яА-ЯіІїЇєЄ']+$"))
            {
                throw new Exception("Ім'я повинно містити лише літери.");
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                throw new Exception("Прізвище не може бути порожнім.");
            }

            if (!Regex.IsMatch(request.LastName, @"^[a-zA-Zа-яА-ЯіІїЇєЄ']+$"))
            {
                throw new Exception("Прізвище повинно містити лише літери.");
            }

            if (request.FacultyId <= 0)
            {
                throw new Exception("Необхідно обрати факультет.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new Exception("Поле не може бути порожнім.");
            }

            if (request.Password.Length < 9)
            {
                throw new Exception("Пароль >= 9 символів.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new Exception("Поле не може бути порожнім.");
            }

            if (!request.Email.EndsWith("@lnu.edu.ua"))
            {
                throw new Exception("Дозволено лише пошту @lnu.edu.ua.");
            }
        }

        private async Task CheckEmailUniquenessAsync(string email, CancellationToken cancellationToken)
        {
            var emailExists = await this._dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);
            if (emailExists)
            {
                throw new Exception("Користувач із таким email уже зареєстрований.");
            }
        }

        private (User, Emailconfirmation) CreateUserAndTokenEntities(RegisterUserCommand request)
        {
            var passwordHash = HashPassword(request.Password);

            var newUser = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = passwordHash,
                FacultyId = request.FacultyId,
                IsEmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
            };

            var confirmationToken = Guid.NewGuid().ToString();
            var tokenEntity = new Emailconfirmation
            {
                User = newUser,
                ConfirmationToken = confirmationToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
            };

            return (newUser, tokenEntity);
        }
    }
}