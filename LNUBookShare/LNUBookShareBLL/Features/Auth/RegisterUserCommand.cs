using MediatR;

namespace LNUBookShareBLL.Features.Auth
{
    public class RegisterUserCommand : IRequest<int>
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int FacultyId { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }
    }
}
