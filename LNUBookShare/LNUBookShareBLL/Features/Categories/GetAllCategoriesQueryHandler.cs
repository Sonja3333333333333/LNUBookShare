using MediatR;
using Microsoft.EntityFrameworkCore;
using LNUBookShareBLL.DTOs;
using LNUBookShareDAL.Models;


namespace LNUBookShareBLL.Features.Categories
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetAllCategoriesQueryHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<IEnumerable<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            return await this._dbContext.Categories
                .AsNoTracking()
                .Select(category => new CategoryDto
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name
                })
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
    }
}