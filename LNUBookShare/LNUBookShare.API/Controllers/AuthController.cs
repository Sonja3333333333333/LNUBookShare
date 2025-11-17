using LNUBookShareDAL.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.API.Controllers
{
    [Route("api/[controller]")] // Адреса /api/auth
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LNUBookShareDbContext _context;

        // Отримуємо доступ до бази
        public AuthController(LNUBookShareDbContext context)
        {
            _context = context;
        }

        // "Ловить" запити /api/auth/confirm?token=...
        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Token is missing.");
            }

            // 1. Шукаємо токен в базі
            var confirmation = await _context.Emailconfirmations
                .FirstOrDefaultAsync(c => c.ConfirmationToken == token);

            if (confirmation == null)
            {
                return NotFound("Invalid token.");
            }

            // 2. Перевіряємо, чи не прострочений
            if (confirmation.ExpiresAt < DateTime.UtcNow)
            {
                return BadRequest("Token has expired. Please request a new one.");
            }

            // 3. Знаходимо користувача
            var user = await _context.Users.FindAsync(confirmation.UserId);
            if (user == null)
            {
                return NotFound("Associated user not found.");
            }

            // 4. ПІДТВЕРДЖУЄМО!
            user.IsEmailConfirmed = true;

            // 5. Видаляємо токен (робимо одноразовим)
            _context.Emailconfirmations.Remove(confirmation);

            await _context.SaveChangesAsync();

            // 6. Повертаємо гарну сторінку "Успіх"
            string htmlResponse = @"
                <html>
                <head><title>Email Confirmed</title></head>
                <body style='font-family: Arial, sans-serif; text-align: center; margin-top: 50px;'>
                    <h1>Success!</h1>
                    <p>Your email has been successfully confirmed. You can now log in to the LNU Book Share application.</p>
                </body>
                </html>";

            return Content(htmlResponse, "text/html");
        }
    }
}