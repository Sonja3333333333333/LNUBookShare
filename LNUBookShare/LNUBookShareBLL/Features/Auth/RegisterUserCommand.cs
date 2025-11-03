using MediatR;
//контейнер для даних, які прийшли з UI

namespace LNUBookShareBLL.Features.Auth
{
    public class RegisterUserCommand : IRequest<int>
    {
        // Дані, необхідні для реєстрації (з твого Вікна 2)
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int FacultyId { get; set; } // Припускаємо, що UI передасть ID факультету
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
