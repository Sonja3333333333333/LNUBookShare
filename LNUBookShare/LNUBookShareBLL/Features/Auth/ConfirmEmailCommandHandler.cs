using MediatR;
using Microsoft.EntityFrameworkCore;

using LNUBookShareDAL.Models;

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
    
            var tokenEntity = await this._dbContext.Emailconfirmations
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.ConfirmationToken == request.ConfirmationToken, cancellationToken);

            if (tokenEntity == null)
            {
                throw new Exception("Недійсний токен підтвердження.");
            }

            if (tokenEntity.ExpiresAt < DateTime.UtcNow)
            {
                _ = this._dbContext.Emailconfirmations.Remove(tokenEntity);
                _ = await this._dbContext.SaveChangesAsync(cancellationToken);
                throw new Exception("Термін дії токена вийшов. Будь ласка, надішліть запит на підтвердження повторно.");
            }

            if (tokenEntity.User == null)
            {
                throw new Exception("Акаунт, пов'язаний з цим токеном, не знайдено.");
            }

            if (tokenEntity.User.IsEmailConfirmed)
            {
                _ = this._dbContext.Emailconfirmations.Remove(tokenEntity);
                _ = await this._dbContext.SaveChangesAsync(cancellationToken);
                return Unit.Value; 
            }

            tokenEntity.User.IsEmailConfirmed = true;

            _ = this._dbContext.Emailconfirmations.Remove(tokenEntity);

            _ = await this._dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}