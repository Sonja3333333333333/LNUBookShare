using LNUBookShareBLL.DTOs;

using MediatR;

namespace LNUBookShareBLL.Features.Categories
{
    public class GetAllCategoriesQuery : IRequest<IEnumerable<CategoryDto>>
    {
    }
}