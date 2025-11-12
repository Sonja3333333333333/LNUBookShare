using MediatR;
using LNUBookShareDAL.Models;
using Microsoft.EntityFrameworkCore; 


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

            int? coverId = null;
            if (!string.IsNullOrEmpty(request.Dto.CoverImagePath))
            {
                // 1. Конвертуємо абсолютний шлях (C:\...) назад у відносний
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string relativePath = Path.GetRelativePath(baseDir, request.Dto.CoverImagePath);

                // 2. ЗАМІНЮЄМО / НА \ (нормалізація)
                // Це гарантує, що "uploads/images/file.png" стане "uploads\images\file.png"
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                // 3. Шукаємо в БД
                var image = await _dbContext.Images
                    .FirstOrDefaultAsync(i => i.ImagePath == relativePath, cancellationToken);

                if (image != null)
                {
                    coverId = image.ImageId;
                }
                else
                {
                    // Це означає, що ми все одно не знайшли зображення
                    Console.WriteLine($"Увага: не вдалося знайти Image для книги за шляхом {relativePath}");
                }
            }

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
                Status = "available",
                CreatedAt = DateTime.UtcNow,

                CoverId = coverId // <-- Прив'язуємо ID обкладинки
            };

            await _dbContext.Books.AddAsync(newBook, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Повертаємо ID нової книги
            return newBook.BookId;
        }
    }
}