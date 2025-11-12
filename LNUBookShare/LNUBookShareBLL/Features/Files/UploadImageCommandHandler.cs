using MediatR;
using LNUBookShareDAL.Models;


namespace LNUBookShareBLL.Features.Files
{
    public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, string>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public UploadImageCommandHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> Handle(UploadImageCommand request, CancellationToken cancellationToken)
        {
            // 1. Шлях для збереження (наприклад, /uploads/images/ у теці запуску)
            // AppDomain.CurrentDomain.BaseDirectory вказує на .../bin/Debug/net7.0-windows
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string uploadDir = Path.Combine(baseDir, "uploads", "images");

            // 2. Створюємо папку, якщо її немає
            Directory.CreateDirectory(uploadDir);

            // 3. Генеруємо унікальне ім'я файлу
            string fileExtension = Path.GetExtension(request.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            string physicalPath = Path.Combine(uploadDir, uniqueFileName);

            string relativePath = Path.Combine("uploads", "images", uniqueFileName);

            // 4. Зберігаємо файл на диск
            await File.WriteAllBytesAsync(physicalPath, request.ImageData, cancellationToken);

           

            // 6. Створюємо запис в таблиці "Images"
            var newImage = new Image
            {
                ImagePath = relativePath,

                UploadedAt = DateTime.UtcNow,
                ImageType = fileExtension
            };

            _dbContext.Images.Add(newImage);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 7. Повертаємо шлях до ViewModel
            return physicalPath;
        }
    }
}