using MediatR;
using System.Collections.Generic;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Faculties
{
    public class GetAllFacultiesQuery : IRequest<IEnumerable<FacultyDto>>
    {
    }
}
