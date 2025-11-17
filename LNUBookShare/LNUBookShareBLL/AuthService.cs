using LNUBookShareDAL.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace LNUBookShareBLL
{
    public class AuthService
    {
        private readonly EmailService _emailService;

        // ----- ЗМІНА 1: Виправлена опечатка -----
        // Було "neondbФ_owner"
        private readonly string _connectionString = "Host=ep-wispy-hat-adm0eu4d-pooler.c-2.us-east-1.aws.neon.tech;" +
                                                    "Database=neondb;" +
                                                    "Username=neondb_owner;" + // <--- ВИПРАВЛЕНО
                                                    "Password=npg_GqkRolz4rhy6;" +
                                                    "SSL Mode=Require;" +
                                                    "Trust Server Certificate=true";

        public AuthService()
        {
            _emailService = new EmailService();
        }

        // Приватний метод для створення DbContext
        private LNUBookShareDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LNUBookShareDbContext>();
            optionsBuilder.UseNpgsql(_connectionString);
            return new LNUBookShareDbContext(optionsBuilder.Options);
        }

        // Головний метод реєстрації (викликається з UI)
        public async Task RegisterUserAsync(string email, string password)
        {
            string token = Guid.NewGuid().ToString();

            // !! УВАГА !!
            // Вставте сюди URL вашого API (напр. https://localhost:7123)
            string confirmationLink = $"https://localhost:7123/api/auth/confirm?token={token}";

            // Ми оголошуємо ці змінні *до* using, щоб мати до них доступ у catch
            User newUser = null;
            Emailconfirmation confirmation = null;
            LNUBookShareDbContext dbContext = null;

            try
            {
                // 2. Створюємо DbContext
                dbContext = CreateDbContext();

                // 3. Створюємо User
                newUser = new User
                {
                    Email = email,
                    PasswordHash = HashPassword(password),
                    IsEmailConfirmed = false,
                    FirstName = "New",
                    LastName = "User",
                    FacultyId = 1,
                    AvatarId = 1
                };

                // 4. Додаємо User в БД
                dbContext.Users.Add(newUser);
                await dbContext.SaveChangesAsync(); // Отримуємо newUser.UserId

                // 5. Створюємо запис Emailconfirmation
                confirmation = new Emailconfirmation
                {
                    UserId = newUser.UserId,
                    ConfirmationToken = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };

                dbContext.Emailconfirmations.Add(confirmation);
                await dbContext.SaveChangesAsync(); // Зберігаємо токен

                // ----- ЗМІНА 2: Додано try...catch -----
                // 6. Намагаємося відправити лист
                await _emailService.SendConfirmationEmailAsync(email, confirmationLink);
            }
            catch (Exception emailEx)
            {
                // Якщо лист не відправився, ми "відкочуємо" зміни в базі
                if (newUser != null && confirmation != null && dbContext != null)
                {
                    // Видаляємо те, що щойно створили
                    dbContext.Emailconfirmations.Remove(confirmation);
                    dbContext.Users.Remove(newUser);
                    await dbContext.SaveChangesAsync();
                }

                // Кидаємо нову, зрозумілу помилку для UI
                throw new Exception($"Failed to send confirmation email: {emailEx.Message}", emailEx);
            }
            finally
            {
                // Завжди закриваємо DbContext
                if (dbContext != null)
                {
                    await dbContext.DisposeAsync();
                }
            }
        }

        // Заглушка для хешування пароля
        private string HashPassword(string password)
        {
            // TODO: Замініть це на реальне хешування (напр. BCrypt.Net)
            return password;
        }
    }
}