using MediatR;

namespace LNUBookShareBLL.Features.Auth
{
    public class ResendConfirmationEmailCommand : IRequest
    {
        public string? Email { get; set; }
    }
}
