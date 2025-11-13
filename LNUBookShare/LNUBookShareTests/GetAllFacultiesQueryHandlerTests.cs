using Xunit;
using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Faculties; // Переконайтесь, що цей using правильний
using LNUBookShareBLL.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using LNUBookShareDAL; // <-- ДОДАЙТЕ ЦЕЙ USING

namespace LNUBookShare.Tests
{
    public class GetAllFacultiesQueryHandlerTests
    {
        private LNUBookShareDbContext _dbContext;
        private DbContextOptions<LNUBookShareDbContext> _options;

        // Конструктор: готує нову БД для кожного тесту
        public GetAllFacultiesQueryHandlerTests()
        {
            this._options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            this._dbContext = new LNUBookShareDbContext(this._options);
        }

        [Fact] // Атрибут xUnit, що позначає цей метод як тест
        public async Task Handle_Should_ReturnAllFaculties_WhenFacultiesExist()
        {
            // --- 1. ARRANGE (Підготовка) ---

            // Додаємо 3 тестові факультети
            await this._dbContext.Faculties.AddRangeAsync(new List<Faculty>
            {
                new Faculty { Name = "Факультет журналістики" },
                new Faculty { Name = "Факультет міжнародних відносин" },
                new Faculty { Name = "Економічний факультет" }
            });
            await this._dbContext.SaveChangesAsync();

            // Створюємо обробник
            var handler = new GetAllFacultiesQueryHandler(this._dbContext);
            var query = new GetAllFacultiesQuery();

            // --- 2. ACT (Виконання) ---
            var result = await handler.Handle(query, CancellationToken.None);

            // --- 3. ASSERT (Перевірка) ---
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoFacultiesExist()
        {
            // --- 1. ARRANGE (Підготовка) ---
            var handler = new GetAllFacultiesQueryHandler(this._dbContext);
            var query = new GetAllFacultiesQuery();

            // --- 2. ACT (Виконання) ---
            var result = await handler.Handle(query, CancellationToken.None);

            // --- 3. ASSERT (Перевірка) ---
            Assert.NotNull(result);
            Assert.Empty(result); // Перевіряємо, що список порожній
        }
    }
}