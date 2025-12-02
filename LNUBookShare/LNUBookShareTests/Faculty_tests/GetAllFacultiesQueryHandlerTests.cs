using LNUBookShareBLL.Features.Faculties;

using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;


namespace LNUBookShareTests.Faculty_tests
{
    public class GetAllFacultiesQueryHandlerTests
    {
        private LNUBookShareDbContext _dbContext;
        private DbContextOptions<LNUBookShareDbContext> _options;


        public GetAllFacultiesQueryHandlerTests()
        {
            this._options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            this._dbContext = new LNUBookShareDbContext(this._options);
        }

        [Fact]
        public async Task Handle_Should_ReturnAllFaculties_WhenFacultiesExist()
        {
            await this._dbContext.Faculties.AddRangeAsync(new List<Faculty>
            {
                new Faculty { Name = "Факультет журналістики" },
                new Faculty { Name = "Факультет міжнародних відносин" },
                new Faculty { Name = "Економічний факультет" }
            });
            await this._dbContext.SaveChangesAsync();


            var handler = new GetAllFacultiesQueryHandler(this._dbContext);
            var query = new GetAllFacultiesQuery();

            var result = await handler.Handle(query, CancellationToken.None);


            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoFacultiesExist()
        {
            var handler = new GetAllFacultiesQueryHandler(this._dbContext);
            var query = new GetAllFacultiesQuery();

            var result = await handler.Handle(query, CancellationToken.None);


            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}