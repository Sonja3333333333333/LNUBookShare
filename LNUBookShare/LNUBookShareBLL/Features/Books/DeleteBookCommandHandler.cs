using MediatR;
using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Books
{
    public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public DeleteBookCommandHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
        {
            // 1. Знаходимо книгу
            var book = await _dbContext.Books
                .FirstOrDefaultAsync(b => b.BookId == request.BookId, cancellationToken);

            if (book == null)
            {
                throw new System.Exception("Книгу не знайдено.");
            }

            // 2. ПЕРЕВІРКА БЕЗПЕКИ: Чи справді цей користувач є власником?
            if (book.OwnerId != request.CurrentUserId)
            {
                throw new System.Exception("Ви не можете видалити книгу, яка вам не належить.");
            }

            // 3. Видаляємо книгу
            _dbContext.Books.Remove(book);

            // (Ми можемо також видалити її з 'Favorite' у всіх,
            // але 'CASCADE' в базі має зробити це автоматично)

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
