using LNUBookShareBLL.DTOs;

using MediatR;

namespace LNUBookShareBLL.Features.Profile
{
    public class GetProfileQuery : IRequest<ProfileDto>
    {
        // Id користувача, чий профіль ми хочемо подивитися
        public int UserId { get; set; }
    }
}
