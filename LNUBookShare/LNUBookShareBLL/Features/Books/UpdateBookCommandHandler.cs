using MediatR;
using Microsoft.EntityFrameworkCore;

using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Books
{
    public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public UpdateBookCommandHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
        {
            // Валідація DTO (тут мають бути перевірки, як у AddBook)
            if (string.IsNullOrWhiteSpace(request.Dto.Title) || string.IsNullOrWhiteSpace(request.Dto.Author) || request.Dto.CategoryId <= 0)
            {
                throw new Exception("Назва, Автор та Категорія є обов'язковими.");
            }

            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.BookId == request.BookId, cancellationToken);

            if (book == null)
            {
                throw new Exception("Книгу не знайдено.");
            }

            // ВАЖЛИВА ПЕРЕВІРКА: Редагувати книгу може тільки її власник
            if (book.OwnerId != request.CurrentUserId)
            {
                throw new Exception("Ви не можете редагувати чужу книгу.");
            }

            // Оновлюємо поля
            book.Title = request.Dto.Title;
            book.Author = request.Dto.Author;
            book.Isbn = request.Dto.Isbn;
            book.Year = request.Dto.Year;
            book.Publisher = request.Dto.Publisher;
            book.Language = request.Dto.Language;
            book.CategoryId = request.Dto.CategoryId;
            book.Status = request.Dto.Status; // "available" or "issued"
            book.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}