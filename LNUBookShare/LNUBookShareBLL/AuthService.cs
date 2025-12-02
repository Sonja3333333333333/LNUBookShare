using LNUBookShareDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShareBLL
{
    public class AuthService
    {
        private readonly EmailService _emailService;

        private readonly string _connectionString = "Host=ep-wispy-hat-adm0eu4d-pooler.c-2.us-east-1.aws.neon.tech;" +
                                                    "Database=neondb;" +
                                                    "Username=neondb_owner;" +
                                                    "Password=npg_GqkRolz4rhy6;" +
                                                    "SSL Mode=Require;" +
                                                    "Trust Server Certificate=true";

        public AuthService()
        {
            _emailService = new EmailService();
        }

        public async Task RegisterUserAsync(string email, string password)
        {
            string token = Guid.NewGuid().ToString();

            string confirmationLink = $"https://localhost:7123/api/auth/confirm?token={token}";

            User newUser = null;
            Emailconfirmation confirmation = null;
            LNUBookShareDbContext dbContext = null;

            try
            {
                dbContext = CreateDbContext();

                newUser = new User
                {
                    Email = email,
                    PasswordHash = HashPassword(password),
                    IsEmailConfirmed = false,
                    FirstName = "New",
                    LastName = "User",
                    FacultyId = 1,
                    AvatarId = 1,
                };

                dbContext.Users.Add(newUser);
                await dbContext.SaveChangesAsync();

                confirmation = new Emailconfirmation
                {
                    UserId = newUser.UserId,
                    ConfirmationToken = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                };

                dbContext.Emailconfirmations.Add(confirmation);
                await dbContext.SaveChangesAsync();

                await _emailService.SendConfirmationEmailAsync(email, confirmationLink);
            }
            catch (Exception emailEx)
            {
                if (newUser != null && confirmation != null && dbContext != null)
                {
                    dbContext.Emailconfirmations.Remove(confirmation);
                    dbContext.Users.Remove(newUser);
                    await dbContext.SaveChangesAsync();
                }

                throw new Exception($"Failed to send confirmation email: {emailEx.Message}", emailEx);
            }
            finally
            {
                if (dbContext != null)
                {
                    await dbContext.DisposeAsync();
                }
            }
        }

        private LNUBookShareDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LNUBookShareDbContext>();
            optionsBuilder.UseNpgsql(_connectionString);
            return new LNUBookShareDbContext(optionsBuilder.Options);
        }

        private string HashPassword(string password)
        {
            return password;
        }
    }
}