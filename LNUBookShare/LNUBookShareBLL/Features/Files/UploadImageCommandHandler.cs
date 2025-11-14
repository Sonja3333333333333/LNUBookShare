using MediatR;
using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Files
{
    public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, string>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public UploadImageCommandHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<string> Handle(UploadImageCommand request, CancellationToken cancellationToken)
        {
            var (physicalPath, relativePath) = await this.SaveFileToDiskAsync(request.ImageData, request.FileName);

            await this.CreateImageEntityAsync(relativePath, request.FileName, cancellationToken);

            return physicalPath;
        }

        private async Task<(string physicalPath, string relativePath)> SaveFileToDiskAsync(byte[] imageData, string fileName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string uploadDir = Path.Combine(baseDir, "uploads", "images");

            Directory.CreateDirectory(uploadDir);

            string fileExtension = Path.GetExtension(fileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            string physicalPath = Path.Combine(uploadDir, uniqueFileName);
            string relativePath = Path.Combine("uploads", "images", uniqueFileName);

            await File.WriteAllBytesAsync(physicalPath, imageData);

            return (physicalPath, relativePath);
        }

        private async Task CreateImageEntityAsync(string relativePath, string originalFileName, CancellationToken cancellationToken)
        {
            var newImage = new Image
            {
                ImagePath = relativePath,
                UploadedAt = DateTime.UtcNow,
                ImageType = Path.GetExtension(originalFileName).ToLower()
            };

            await this._dbContext.Images.AddAsync(newImage, cancellationToken);
            await this._dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}