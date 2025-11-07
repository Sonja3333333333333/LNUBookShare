using MediatR;
using LNUBookShareDAL.Models;


namespace LNUBookShareBLL.Features.Books
{
    public class AddBookCommandHandler : IRequestHandler<AddBookCommand, int>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public AddBookCommandHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> Handle(AddBookCommand request, CancellationToken cancellationToken)
        {
            // Валідація DTO
            if (string.IsNullOrWhiteSpace(request.Dto.Title) || string.IsNullOrWhiteSpace(request.Dto.Author) || request.Dto.CategoryId <= 0)
            {
                throw new Exception("Назва, Автор та Категорія є обов'язковими.");
            }

            // Створюємо нову сутність Книги
            var newBook = new Book
            {
                OwnerId = request.OwnerUserId,
                Title = request.Dto.Title,
                Author = request.Dto.Author,
                Isbn = request.Dto.Isbn,
                Year = request.Dto.Year,
                Publisher = request.Dto.Publisher,
                Language = request.Dto.Language,
                CategoryId = request.Dto.CategoryId,

                // Згідно з вимогами, книга при додаванні завжди "доступна"
                Status = "available",
                CreatedAt = DateTime.UtcNow

                // TODO: Додати логіку для CoverId
                // CoverId = request.Dto.CoverId 
            };

            await _dbContext.Books.AddAsync(newBook, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Повертаємо ID нової книги
            return newBook.BookId;
        }
    }
}