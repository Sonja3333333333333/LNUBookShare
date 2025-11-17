//Handle_ShouldSaveFileToDisk_AndCreateImageEntityInDb: "Щасливий шлях" №1.Перевіряє, що обробник одночасно робить дві речі: 1) реально створює файл на диску у папці uploads/images та 2) створює запис про цей файл у таблиці Images в базі даних.

//Handle_ShouldThrowException_WhenImageDataIsNull: "Сумний шлях" №1.Перевіряє, що код кидає помилку (очікувано ArgumentNullException), якщо ми намагаємося завантажити файл, передавши null замість даних файлу (ImageData).


using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Files;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LNUBookShareBLL.Tests.Files
{
    public class UploadImageCommandHandlerTests
    {
        private LNUBookShareDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new LNUBookShareDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        // === ТЕСТ 1: "Щасливий шлях" (Файл збережено, запис в БД створено) ===
        [Fact]
        public async Task Handle_ShouldSaveFileToDisk_AndCreateImageEntityInDb()
        {
            // Arrange
            await using var context = GetInMemoryDbContext();
            var handler = new UploadImageCommandHandler(context);

            var command = new UploadImageCommand
            {
                FileName = "test-image.png",
                ImageData = new byte[] { 1, 2, 3, 4, 5 }
            };

            string createdPhysicalPath = null; 

            try
            {
                createdPhysicalPath = await handler.Handle(command, CancellationToken.None);

                createdPhysicalPath.Should().NotBeNullOrEmpty();
                createdPhysicalPath.Should().Contain(command.FileName.Substring(command.FileName.IndexOf("."))); 
                File.Exists(createdPhysicalPath).Should().BeTrue(); 

                var imageInDb = await context.Images.FirstOrDefaultAsync();
                imageInDb.Should().NotBeNull();
                imageInDb.ImagePath.Should().Contain(@"uploads\images\");
                imageInDb.ImagePath.Should().EndWith(".png");
                imageInDb.ImageType.Should().Be(".png");
            }
            finally
            {
                if (File.Exists(createdPhysicalPath))
                {
                    File.Delete(createdPhysicalPath);
                }
            }
        }

        // === ТЕСТ 2: "Сумний шлях" (Неправильні дані) ===
        [Fact]
        public async Task Handle_ShouldThrowException_WhenImageDataIsNull()
        {
            await using var context = GetInMemoryDbContext();
            var handler = new UploadImageCommandHandler(context);

            var command = new UploadImageCommand
            {
                FileName = "no-data.png",
                ImageData = null 
            };

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentNullException>();

            (await context.Images.CountAsync()).Should().Be(0);
        }
    }
}