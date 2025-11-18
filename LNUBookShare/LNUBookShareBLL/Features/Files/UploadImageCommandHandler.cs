using MediatR;
using LNUBookShareDAL.Models;
using CloudinaryDotNet; 
using CloudinaryDotNet.Actions;

namespace LNUBookShareBLL.Features.Files
{
    public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, string>
    {
        private readonly LNUBookShareDbContext _dbContext;
        private readonly Cloudinary _cloudinary;

        public UploadImageCommandHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;

            var account = new Account(
                "dvyt88mow",
                "187511349473781",
                "e9y4W49R3g-7kVASgj8ImW-YR6k");

            this._cloudinary = new Cloudinary(account);
        }

        public async Task<string> Handle(UploadImageCommand request, CancellationToken cancellationToken)
        {
            
            var imageUrl = await this.UploadToCloudinaryAsync(request.ImageData, request.FileName);

      
            await this.CreateImageEntityAsync(imageUrl, request.FileName, cancellationToken);

   
            return imageUrl;
        }


        private async Task<string> UploadToCloudinaryAsync(byte[] imageData, string fileName)
        {
            using (var stream = new MemoryStream(imageData))
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(fileName, stream),
                    Folder = "lnu_book_share"
                };

                var uploadResult = await this._cloudinary.UploadAsync(uploadParams);

                return uploadResult.SecureUrl.ToString();
            }
        }

        private async Task CreateImageEntityAsync(string imageUrl, string originalFileName, CancellationToken cancellationToken)
        {
            var newImage = new Image
            {
                ImagePath = imageUrl,
                UploadedAt = DateTime.UtcNow,
                ImageType = Path.GetExtension(originalFileName).ToLower()
            };

            await this._dbContext.Images.AddAsync(newImage, cancellationToken);
            await this._dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}