using MediatR;

namespace LNUBookShareBLL.Features.Auth
{
    public class ConfirmEmailCommand : IRequest
    {
        public string? ConfirmationToken { get; set; }
    }
}
