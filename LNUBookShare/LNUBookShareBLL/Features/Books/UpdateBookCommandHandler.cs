using LNUBookShareBLL.Common;
using LNUBookShareBLL.DTOs;

using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareBLL.Features.Books
{
    public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public UpdateBookCommandHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Unit> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
        {
            var book = await this.GetBookAsync(request.BookId, cancellationToken);

            this.ValidateOwnership(book, request.CurrentUserId);

            int? coverId = await this.GetCoverIdAsync(request.Dto.CoverImagePath, cancellationToken);

            this.MapDtoToBook(request.Dto, book, coverId);

            await this._dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        private async Task<Book> GetBookAsync(int bookId, CancellationToken cancellationToken)
        {
            var book = await this._dbContext.Books
                .FirstOrDefaultAsync(book => book.BookId == bookId, cancellationToken);

            if (book == null)
            {
                throw new Exception("Книгу не знайдено.");
            }

            return book;
        }

        private void ValidateOwnership(Book book, int currentUserId)
        {
            if (book.OwnerId != currentUserId)
            {
                throw new Exception("Ви не можете редагувати чужу книгу.");
            }
        }

        private async Task<int?> GetCoverIdAsync(string? imagePath, CancellationToken token)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return null;
            }

            string relativePath = PathHelper.ConvertToRelativePath(imagePath);
            var image = await this._dbContext.Images
                .FirstOrDefaultAsync(i => i.ImagePath == relativePath, token);

            if (image == null)
            {
                Console.WriteLine($"Увага: не вдалося знайти Image для книги за шляхом {relativePath}");
                return null;
            }

            return image.ImageId;
        }

        private void MapDtoToBook(BookEditDto dto, Book book, int? coverId)
        {
            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Isbn = dto.Isbn;
            book.Year = dto.Year;
            book.Publisher = dto.Publisher;
            book.Language = dto.Language;
            book.CategoryId = dto.CategoryId;
            book.Status = dto.Status;
            book.UpdatedAt = DateTime.UtcNow;

            if (coverId.HasValue)
            {
                book.CoverId = coverId;
            }
        }
    }
}