using MediatR;
using LNUBookShareBLL.DTOs;

namespace LNUBookShareBLL.Features.Auth
{ 
    public class LoginUserQuery : IRequest<LoginResultDto>
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}