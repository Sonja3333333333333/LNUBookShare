using LNUBookShareDAL.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LNUBookShareDbContext _context;

        public AuthController(LNUBookShareDbContext context)
        {
            this._context = context;
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return this.BadRequest("Token is missing.");
            }

            var confirmation = await this._context.Emailconfirmations
                .FirstOrDefaultAsync(c => c.ConfirmationToken == token);

            if (confirmation == null)
            {
                return this.NotFound("Invalid token.");
            }

            if (confirmation.ExpiresAt < DateTime.UtcNow)
            {
                return this.BadRequest("Token has expired. Please request a new one.");
            }

            var user = await this._context.Users.FindAsync(confirmation.UserId);
            if (user == null)
            {
                return this.NotFound("Associated user not found.");
            }

            user.IsEmailConfirmed = true;

            this._context.Emailconfirmations.Remove(confirmation);

            await this._context.SaveChangesAsync();

            string htmlResponse = @"
                <html>
                <head><title>Email Confirmed</title></head>
                <body style='font-family: Arial, sans-serif; text-align: center; margin-top: 50px;'>
                    <h1>Success!</h1>
                    <p>Your email has been successfully confirmed. You can now log in to the LNU Book Share application.</p>
                </body>
                </html>";

            return this.Content(htmlResponse, "text/html");
        }
    }
}