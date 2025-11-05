using MediatR;
using LNUBookShareBLL.DTOs;
using LNUBookShareDAL; // 👈 Переконайся, що твій DbContext тут
using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;

namespace LNUBookShareBLL.Features.Faculties
{
    /// <summary>
    /// Обробник, який *вміє* виконувати GetAllFacultiesQuery
    /// </summary>
    public class GetAllFacultiesQueryHandler : IRequestHandler<GetAllFacultiesQuery, List<FacultyDto>>
    {
        private readonly LNUBookShareDbContext _dbContext;

        public GetAllFacultiesQueryHandler(LNUBookShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<FacultyDto>> Handle(GetAllFacultiesQuery request, CancellationToken cancellationToken)
        {
            // 1. Звертаємось до таблиці Faculties у базі
            return await _dbContext.Faculties
                .OrderBy(f => f.Name) // Сортуємо за алфавітом
                .Select(f => new FacultyDto // 2. Перетворюємо (Проектуємо) на DTO
                {
                    FacultyId = f.FacultyId,
                    Name = f.Name
                })
                .ToListAsync(cancellationToken); // 3. Повертаємо список
        }
    }
}