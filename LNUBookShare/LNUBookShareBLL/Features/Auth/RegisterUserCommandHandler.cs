using MediatR;
using LNUBookShareDAL.Models; 
using Microsoft.EntityFrameworkCore;
using static BCrypt.Net.BCrypt;
using System.Text.RegularExpressions;

namespace LNUBookShareBLL.Features.Auth
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, int>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public RegisterUserCommandHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        //Головний метод
        public async Task<int> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            //валідація
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

            var emilExists = await this._dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);

            if(emilExists)
            {
                throw new Exception("Користувач із таким email уже зареєстрований.");
            }

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

            _ = await this._dbContext.Users.AddAsync(newUser, cancellationToken);

            var confirmationToken = Guid.NewGuid().ToString(); //генеруємо токен

            var tokenEntity = new Emailconfirmation
            {
                User = newUser,
                ConfirmationToken = confirmationToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _ = await this._dbContext.Emailconfirmations.AddAsync(tokenEntity, cancellationToken);

            _ = await this._dbContext.SaveChangesAsync(cancellationToken);


            //Тут треба викликати сервіс відправки пошти для надіслання підтверження

            return newUser.UserId;

        }

    }
}
