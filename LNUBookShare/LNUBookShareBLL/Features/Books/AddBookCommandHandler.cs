using MediatR;
using LNUBookShareDAL.Models;
using Microsoft.EntityFrameworkCore;
using LNUBookShareBLL.Common;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Books
{
    public class AddBookCommandHandler : IRequestHandler<AddBookCommand, int>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public AddBookCommandHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<int> Handle(AddBookCommand request, CancellationToken cancellationToken)
        {
      
            this.ValidateRequest(request.Dto);

            
            int? coverId = await this.GetCoverIdAsync(request.Dto.CoverImagePath, cancellationToken);

            
            var newBook = this.MapDtoToBook(request, coverId);

            
            await this._dbContext.Books.AddAsync(newBook, cancellationToken);
            await this._dbContext.SaveChangesAsync(cancellationToken);

            return newBook.BookId;
        }


        private void ValidateRequest(AddBookDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Author) || dto.CategoryId <= 0)
            {
                throw new Exception("Назва, Автор та Категорія є обов'язковими.");
            }
        }

        private async Task<int?> GetCoverIdAsync(string? imagePath, CancellationToken token)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return null;
            }

            var relativePath = PathHelper.ConvertToRelativePath(imagePath);
            var image = await this._dbContext.Images
                .FirstOrDefaultAsync(i => i.ImagePath == relativePath, token);

            if (image == null)
            {
                Console.WriteLine($"Увага: не вдалося знайти Image для книги за шляхом {relativePath}");
                return null;
            }

            return image.ImageId;
        }

        private Book MapDtoToBook(AddBookCommand request, int? coverId)
        {
            return new Book
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
                CoverId = coverId
            };
        }
    }
}