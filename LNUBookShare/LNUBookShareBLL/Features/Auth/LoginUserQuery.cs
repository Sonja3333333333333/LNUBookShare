using LNUBookShareBLL.DTOs;

using MediatR;

namespace LNUBookShareBLL.Features.Auth
{
    public class LoginUserQuery : IRequest<LoginResultDto>
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}