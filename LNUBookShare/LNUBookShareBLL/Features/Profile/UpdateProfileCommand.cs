using MediatR;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Profile
{
    /// <summary>
    /// Команда для збереження оновлених даних профілю.
    /// Приймає ID користувача, якого редагують, та DTO з новими даними.
    /// </summary>
    public class UpdateProfileCommand : IRequest
    {
        public int UserId { get; set; }
        public ProfileEditDto? Dto { get; set; }
    }
}