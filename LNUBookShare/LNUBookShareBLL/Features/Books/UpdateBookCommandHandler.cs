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
            var book = await _dbContext.Books
                .FirstOrDefaultAsync(b => b.BookId == request.BookId, cancellationToken);

            if (book == null)
            {
                throw new Exception("Книгу не знайдено.");
            }

            if (book.OwnerId != request.CurrentUserId)
            {
                throw new Exception("Ви не можете редагувати чужу книгу.");
            }

            // Оновлюємо текстові поля
            book.Title = request.Dto.Title;
            book.Author = request.Dto.Author;
            book.Isbn = request.Dto.Isbn;
            book.Year = request.Dto.Year;
            book.Publisher = request.Dto.Publisher;
            book.Language = request.Dto.Language;
            book.CategoryId = request.Dto.CategoryId;
            book.Status = request.Dto.Status;
            book.UpdatedAt = DateTime.UtcNow;

            // Оновлюємо обкладинку (та сама логіка, що й в AddBook)
            if (!string.IsNullOrEmpty(request.Dto.CoverImagePath))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string relativePath = Path.GetRelativePath(baseDir, request.Dto.CoverImagePath);
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                var image = await _dbContext.Images
                    .FirstOrDefaultAsync(i => i.ImagePath == relativePath, cancellationToken);

                if (image != null)
                {
                    book.CoverId = image.ImageId;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}