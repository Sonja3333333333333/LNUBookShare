using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;


namespace LNUBookShareBLL.Features.Auth
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Unit>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public ConfirmEmailCommandHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Unit> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var tokenEntity = await this.GetTokenWithUserAsync(request.ConfirmationToken, cancellationToken);
            await this.ValidateTokenAsync(tokenEntity, cancellationToken);
            await this.HandleConfirmationAsync(tokenEntity, cancellationToken);

            return Unit.Value;
        }

        private async Task<Emailconfirmation> GetTokenWithUserAsync(string confirmationToken, CancellationToken cancellationToken)
        {
            return await this._dbContext.Emailconfirmations
                .Include(token => token.User)
                .FirstOrDefaultAsync(token => token.ConfirmationToken == confirmationToken, cancellationToken);
        }

        private async Task ValidateTokenAsync(Emailconfirmation tokenEntity, CancellationToken cancellationToken)
        {
            if (tokenEntity == null)
            {
                throw new Exception("Недійсний токен підтвердження.");
            }

            if (tokenEntity.ExpiresAt < DateTime.UtcNow)
            {
                this._dbContext.Emailconfirmations.Remove(tokenEntity);
                await this._dbContext.SaveChangesAsync(cancellationToken);
                throw new Exception("Термін дії токена вийшов. Будь ласка, надішліть запит на підтвердження повторно.");
            }

            if (tokenEntity.User == null)
            {
                throw new Exception("Акаунт, пов'язаний з цим токеном, не знайдено.");
            }
        }

        private async Task HandleConfirmationAsync(Emailconfirmation tokenEntity, CancellationToken cancellationToken)
        {
            if (tokenEntity.User.IsEmailConfirmed == false)
            {
                tokenEntity.User.IsEmailConfirmed = true;
            }

            this._dbContext.Emailconfirmations.Remove(tokenEntity);

            await this._dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}