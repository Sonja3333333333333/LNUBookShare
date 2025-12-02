using LNUBookShareBLL.DTOs;

using MediatR;

namespace LNUBookShareBLL.Features.Faculties
{
    public class GetAllFacultiesQuery : IRequest<List<FacultyDto>>
    {
    }
}