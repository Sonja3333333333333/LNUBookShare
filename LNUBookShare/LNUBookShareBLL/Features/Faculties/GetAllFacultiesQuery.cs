using MediatR; 
using LNUBookShareBLL.DTOs; 

namespace LNUBookShareBLL.Features.Faculties
{
  
    public class GetAllFacultiesQuery : IRequest<List<FacultyDto>>
    {
       
    }
}