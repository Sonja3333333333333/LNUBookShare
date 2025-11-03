using MediatR;
using Microsoft.EntityFrameworkCore;

using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Auth
{
    //знаходить користувача перевіряє чи не зарано для повторної відправки і оновлює токен у базі
    public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand>
    {
        private readonly LNUBookShareDbContext _dbContext;
        // private readonly IEmailService _emailService; // Це знадобиться пізніше

        // public ResendConfirmationEmailCommandHandler(LNUBookShareDbContext dbContext, IEmailService emailService)
        public ResendConfirmationEmailCommandHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
            // _emailService = emailService;
        }

        public async Task<Unit> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
        {

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.EndsWith("@lnu.edu.ua"))
            {
                throw new Exception("Введіть коректну пошту @lnu.edu.ua.");
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null)
            {
                throw new Exception("Користувача не знайдено");
            }

            if (user.IsEmailConfirmed)
            {
                throw new Exception("Цей акаунт вже підтверджено.");
            }

            var tokenEntity = await _dbContext.Emailconfirmations
                .FirstOrDefaultAsync(t => t.UserId == user.UserId, cancellationToken);

            if (tokenEntity == null)
            {
                tokenEntity = new LNUBookShareDAL.Models.Emailconfirmation
                {
                    UserId = user.UserId
                };
                await _dbContext.Emailconfirmations.AddAsync(tokenEntity, cancellationToken);
            }

            // 5. БІЗНЕС-ЛОГІКА: Перевірка таймера (60 сек)
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            if (tokenEntity.CreatedAt > oneMinuteAgo)
            {
                throw new Exception("Повторно надіслати лист можна лише раз на хвилину.");
            }

            tokenEntity.ConfirmationToken = Guid.NewGuid().ToString();
            tokenEntity.CreatedAt = DateTime.UtcNow;
            tokenEntity.ExpiresAt = DateTime.UtcNow.AddHours(24); // Даємо ще 24 години

            await _dbContext.SaveChangesAsync(cancellationToken);

            // 8. Відправка email (коли сервіс буде готовий)
            // await _emailService.SendConfirmationEmail(
            //     user.Email,
            //     tokenEntity.ConfirmationToken
            // );

            return Unit.Value; // Означає "void" або "успіх"
        }
    }
}
    