//using LNUBookShareBLL.Dtos;
using LNUBookShareBLL.DTOs;

using MediatR;

namespace LNUBookShareBLL.Features.Profile
{
    /// <summary>
    /// Запит на отримання поточних даних користувача для редагування.
    /// </summary>
    public class GetProfileForEditQuery : IRequest<ProfileEditDto>
    {
        public int UserId { get; set; }
    }
}