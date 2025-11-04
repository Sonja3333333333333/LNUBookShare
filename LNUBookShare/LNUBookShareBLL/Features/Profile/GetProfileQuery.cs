using MediatR;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Profile
{
    public class GetProfileQuery : IRequest<ProfileDto>
    {
        // Id користувача, чий профіль ми хочемо подивитися
        public int UserId { get; set; }
    }
}
