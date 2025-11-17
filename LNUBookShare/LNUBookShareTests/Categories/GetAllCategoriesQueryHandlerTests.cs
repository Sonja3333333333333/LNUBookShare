using Xunit;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Categories;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace LNUBookShareTests.Categories
{
    public class GetAllCategoriesQueryHandlerTests : TestBase
    {
        [Fact]
        public async Task Handle_CategoriesExist_ReturnsAllCategories()
        {
            // Arrange
            DbContext.Categories.AddRange(
                new Category { Name = "Програмування" },
                new Category { Name = "Математика" },
                new Category { Name = "Історія" }
            );
            await DbContext.SaveChangesAsync();

            var handler = new GetAllCategoriesQueryHandler(DbContext);
            var query = new GetAllCategoriesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, c => c.Name == "Програмування");
        }

        [Fact]
        public async Task Handle_NoCategories_ReturnsEmptyList()
        {
            // Arrange
            var handler = new GetAllCategoriesQueryHandler(DbContext);
            var query = new GetAllCategoriesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_CategoriesExist_ReturnsOrderedByName()
        {
            // Arrange
            DbContext.Categories.AddRange(
                new Category { Name = "Zоологія" },
                new Category { Name = "Анатомія" },
                new Category { Name = "Біологія" }
            );
            await DbContext.SaveChangesAsync();

            var handler = new GetAllCategoriesQueryHandler(DbContext);
            var query = new GetAllCategoriesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            var list = result.ToList();
            Assert.Equal("Анатомія", list[0].Name);
            Assert.Equal("Біологія", list[1].Name);
            Assert.Equal("Zоологія", list[2].Name);
        }
    }
}