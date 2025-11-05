using MediatR; // 👈 1. ДОДАЙ ЦЕЙ USING
using LNUBookShareBLL.DTOs; // 👈 2. ДОДАЙ ЦЕЙ USING

namespace LNUBookShareBLL.Features.Faculties
{
  
    public class GetAllFacultiesQuery : IRequest<List<FacultyDto>>
    {
        // Пусто
    }
}