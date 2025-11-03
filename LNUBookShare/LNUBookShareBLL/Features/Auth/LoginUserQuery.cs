using MediatR;
using LNUBookShareBLL.Dtos;

namespace LNUBookShareBLL.Features.Auth
{
    // Запит який UI надсилає в BLL
    public class LoginUserQuery : IRequest<LoginResultDto>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}