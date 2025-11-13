// LNUBookShareBLL/AuthService.cs

using LNUBookShareDAL.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // <--- ВАЖЛИВО: додайте цей using

namespace LNUBookShareBLL
{
    public class AuthService
    {
        private readonly EmailService _emailService;

        // !! ВАЖЛИВО !!
        // Вам потрібно вставити сюди ваш реальний рядок підключення до БД
        // (Пізніше ми перенесемо його в конфігураційний файл)
        private readonly string _connectionString = "Host=localhost;Database=LNUBookShareDb;Username=postgres;Password=your_password";

        public AuthService()
        {
            this._emailService = new EmailService();
        }

        // --- НОВИЙ ПРИВАТНИЙ МЕТОД ---
        // Він створює DbContext з правильними опціями
        private LNUBookShareDbContext CreateDbContext()
        {
            // 1. Створюємо "будівельник" опцій
            var optionsBuilder = new DbContextOptionsBuilder<LNUBookShareDbContext>();

            // 2. Вказуємо, що ми використовуємо Npgsql (PostgreSQL) 
            //    і передаємо йому рядок підключення
            _ = optionsBuilder.UseNpgsql(this._connectionString);

            // 3. Створюємо і повертаємо DbContext з цими опціями
            return new LNUBookShareDbContext(optionsBuilder.Options);
        }
        // --- КІНЕЦЬ НОВОГО МЕТОДУ ---


        // ЦЕЙ МЕТОД ВИКЛИКАЄ ВАШ UI
        public async Task RegisterUserAsync(string email, string password)
        {
            // 1. Створюємо токен і посилання
            string token = Guid.NewGuid().ToString();
            string confirmationLink = $"https://api.vash-proekt.com/api/auth/confirm?token={token}";

            // 2. Створюємо DbContext за допомогою нашого нового методу
            using (var dbContext = this.CreateDbContext())
            {
                // 3. Створюємо об'єкт User 
                var newUser = new User
                {
                    Email = email,
                    PasswordHash = this.HashPassword(password), // !! Потрібно реалізувати хешування
                    IsEmailConfirmed = false, // ГОЛОВНЕ: ще не підтверджений
                    FirstName = "New", // Можете додати більше полів
                    LastName = "User",
                    FacultyId = 1, // Тимчасово (потрібно взяти реальний ID)
                    AvatarId = 1   // Тимчасово (потрібно взяти реальний ID)
                };

                // 4. Додаємо користувача в БД
                _ = dbContext.Users.Add(newUser);
                _ = await dbContext.SaveChangesAsync(); // Зберігаємо, щоб отримати newUser.UserId

                // 5. Створюємо запис для токена в таблиці Emailconfirmation
                var confirmation = new Emailconfirmation
                {
                    UserId = newUser.UserId, // Прив'язуємо до щойно створеного юзера
                    ConfirmationToken = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(24) // Токен діє 24 години
                };

                _ = dbContext.Emailconfirmations.Add(confirmation);
                _ = await dbContext.SaveChangesAsync(); // Зберігаємо токен

                // 6. ТІЛЬКИ ЯКЩО ВСЕ ЗБЕРЕГЛОСЯ, відправляємо лист
                await this._emailService.SendConfirmationEmailAsync(email, confirmationLink);
            }
        }

        // Функція для хешування пароля (дуже спрощена, 
        // для реальних проектів використовуйте BCrypt.Net)
        private string HashPassword(string password)
        {
            // ЗАМІНІТЬ ЦЕ НА РЕАЛЬНЕ ХЕШУВАННЯ
            // Наприклад: return BCrypt.Net.BCrypt.HashPassword(password);
            return password;
        }
    }
}