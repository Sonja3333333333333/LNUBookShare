using MediatR;
using LNUBookShareBLL.DTOs;
using System.Collections.Generic;

namespace LNUBookShareBLL.Features.Categories
{
    public class GetAllCategoriesQuery : IRequest<IEnumerable<CategoryDto>>
    {
    }
}