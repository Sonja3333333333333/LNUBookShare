using LNUBookShareBLL.DTOs;

using LNUBookShareDAL.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareBLL.Features.Faculties
{
    public class GetAllFacultiesQueryHandler : IRequestHandler<GetAllFacultiesQuery, List<FacultyDto>>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetAllFacultiesQueryHandler(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<List<FacultyDto>> Handle(GetAllFacultiesQuery request, CancellationToken cancellationToken)
        {
            return await this._dbContext.Faculties
                .OrderBy(faculty => faculty.Name)
                .Select(faculty => new FacultyDto
                {
                    FacultyId = faculty.FacultyId,
                    Name = faculty.Name
                })
                .ToListAsync(cancellationToken);
        }
    }
}