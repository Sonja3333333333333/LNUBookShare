using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;


namespace LNUBookShareBLL.Features.Auth
{
    public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public ResendConfirmationEmailCommandHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Unit> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
        {
            this.ValidateRequest(request);

            var user = await this.GetUserAndValidateStateAsync(request.Email, cancellationToken);

            var tokenEntity = await this.GetAndValidateTokenAsync(user.UserId, cancellationToken);

            await this.UpdateAndSaveTokenAsync(tokenEntity);

            return Unit.Value;
        }

        private void ValidateRequest(ResendConfirmationEmailCommand request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.EndsWith("@lnu.edu.ua"))
            {
                throw new Exception("Введіть коректну пошту @lnu.edu.ua.");
            }
        }

        private async Task<User> GetUserAndValidateStateAsync(string email, CancellationToken cancellationToken)
        {
            var user = await this._dbContext.Users
                .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

            if (user == null)
            {
                throw new Exception("Користувача не знайдено");
            }

            if (user.IsEmailConfirmed)
            {
                throw new Exception("Цей акаунт вже підтверджено.");
            }

            return user;
        }

        private async Task<Emailconfirmation> GetAndValidateTokenAsync(int userId, CancellationToken cancellationToken)
        {
            var tokenEntity = await this._dbContext.Emailconfirmations
                .FirstOrDefaultAsync(token => token.UserId == userId, cancellationToken);

            if (tokenEntity == null)
            {
                tokenEntity = new Emailconfirmation
                {
                    UserId = userId
                };
                await this._dbContext.Emailconfirmations.AddAsync(tokenEntity, cancellationToken);
            }

            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            if (tokenEntity.CreatedAt > oneMinuteAgo)
            {
                throw new Exception("Повторно надіслати лист можна лише раз на хвилину.");
            }

            return tokenEntity;
        }

        private async Task UpdateAndSaveTokenAsync(Emailconfirmation tokenEntity)
        {
            tokenEntity.ConfirmationToken = Guid.NewGuid().ToString();
            tokenEntity.CreatedAt = DateTime.UtcNow;
            tokenEntity.ExpiresAt = DateTime.UtcNow.AddHours(24);

            await this._dbContext.SaveChangesAsync();
        }
    }
}