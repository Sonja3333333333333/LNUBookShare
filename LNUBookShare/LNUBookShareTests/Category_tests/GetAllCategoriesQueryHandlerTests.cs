using LNUBookShareBLL.Features.Categories;

using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareTests.Categories
{
    public class GetAllCategoriesQueryHandlerTests
    {
        [Fact]
        public async Task Handle_CategoriesExist_ReturnsAllCategories()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedCategories(context);

            var handler = new GetAllCategoriesQueryHandler(context);
            var query = new GetAllCategoriesQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(5, result.Count());
            Assert.Contains(result, category => category.Name == "Програмування");
        }

        [Fact]
        public async Task Handle_NoCategories_ReturnsEmptyList()
        {
            await using var context = this.GetInMemoryDbContext();

            var handler = new GetAllCategoriesQueryHandler(context);
            var query = new GetAllCategoriesQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_CategoriesExist_ReturnsOrderedByName()
        {
            await using var context = this.GetInMemoryDbContext();
            await this.SeedCategories(context);

            var handler = new GetAllCategoriesQueryHandler(context);
            var query = new GetAllCategoriesQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            var list = result.ToList();
            Assert.Equal("Анатомія", list[0].Name);
            Assert.Equal("Біологія", list[1].Name);
            Assert.Equal("Історія", list[2].Name);
            Assert.Equal("Математика", list[3].Name);
            Assert.Equal("Програмування", list[4].Name);
        }

        private LNUBookShareDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var context = new LNUBookShareDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private async Task SeedCategories(LNUBookShareDbContext context)
        {
            context.Categories.AddRange(
                new Category { Name = "Математика" },
                new Category { Name = "Програмування" },
                new Category { Name = "Історія" },
                new Category { Name = "Анатомія" },
                new Category { Name = "Біологія" });
            await context.SaveChangesAsync();
        }
    }
}