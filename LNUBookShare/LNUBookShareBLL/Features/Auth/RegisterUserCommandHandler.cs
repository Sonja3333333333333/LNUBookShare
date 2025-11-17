using MediatR;
using LNUBookShareDAL.Models;
using Microsoft.EntityFrameworkCore;
using static BCrypt.Net.BCrypt;
using System.Text.RegularExpressions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LNUBookShareBLL.Features.Auth
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, int>
    {
        private readonly LNUBookShareDbContext _dbContext;
        private readonly EmailService _emailService; // <--- ЗМІНА 1: Додано EmailService

        // ЗМІНА 2: EmailService отримується через конструктор
        public RegisterUserCommandHandler(LNUBookShareDbContext dbContext, EmailService emailService)
        {
            this._dbContext = dbContext;
            this._emailService = emailService; // <---
        }

        public async Task<int> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            this.ValidateRequest(request);
            await this.CheckEmailUniquenessAsync(request.Email, cancellationToken);

            var (newUser, tokenEntity) = this.CreateUserAndTokenEntities(request);

            await this._dbContext.Users.AddAsync(newUser, cancellationToken);
            await this._dbContext.Emailconfirmations.AddAsync(tokenEntity, cancellationToken);

            // ----- ЗМІНА 3: Додано відправку листа та обробку помилок -----

            // Створюємо посилання (вставте URL вашого API)
            string confirmationLink = $"https://localhost:7163/api/auth/confirm?token={tokenEntity.ConfirmationToken}";
            try
            {
                // 1. Зберігаємо користувача і токен в базі
                await this._dbContext.SaveChangesAsync(cancellationToken);

                // 2. Намагаємося відправити лист
                await this._emailService.SendConfirmationEmailAsync(request.Email, confirmationLink);
            }
            catch (Exception ex)
            {
                // 3. ЯКЩО лист не відправився, "відкочуємо" зміни (видаляємо юзера)
                // Це запобіжить помилці "Користувач уже існує"
                _dbContext.Users.Remove(newUser);
                _dbContext.Emailconfirmations.Remove(tokenEntity);
                await this._dbContext.SaveChangesAsync(); // Зберігаємо видалення

                // 4. Кидаємо помилку для UI
                throw new Exception($"Не вдалося надіслати лист підтвердження: {ex.Message}");
            }
            // ----- КІНЕЦЬ ЗМІНИ -----

            return newUser.UserId;
        }

        // ... (решта коду ValidateRequest, CheckEmailUniquenessAsync, CreateUserAndTokenEntities залишається без змін) ...
        // (Я прибрав їх звідси для короткого огляду, але вони мають бути у файлі)

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
                CreatedAt = DateTime.UtcNow
            };

            var confirmationToken = Guid.NewGuid().ToString();
            var tokenEntity = new Emailconfirmation
            {
                User = newUser,
                ConfirmationToken = confirmationToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            return (newUser, tokenEntity);
        }
    }
}