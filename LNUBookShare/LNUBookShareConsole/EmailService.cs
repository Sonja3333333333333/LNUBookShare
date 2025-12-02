// Це початок файлу LNUBookShareBLL/Services/EmailService.cs
using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;

// Переконайтеся, що простір імен (namespace)
// відповідає вашому проекту
namespace LNUBookShareBLL
{
    public class EmailService
    {
        // Наш головний метод для відправки листа
        // Він асинхронний (async Task), бо відправка пошти
        // може зайняти час
        public async Task SendConfirmationEmailAsync(string userEmail, string confirmationLink)
        {
            // 1. Створюємо сам лист
            var message = new MimeMessage();

            // ВАЖЛИВО: це має бути пошта, з якої ви відправляєте
            message.From.Add(new MailboxAddress("LNU Book Share", "vasha.poshta@gmail.com"));
            message.To.Add(new MailboxAddress("Новий Користувач", userEmail));
            message.Subject = "Підтвердження реєстрації LNU Book Share";

            // 2. Створюємо тіло листа (HTML для гарного посилання)
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
                <h1>Дякуємо за реєстрацію!</h1>
                <p>Будь ласка, натисніть на посилання нижче, щоб активувати ваш акаунт:</p>
                <a href='{confirmationLink}' 
                   style='padding: 10px 15px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>
                   Активувати акаунт
                </a>
            ";
            message.Body = bodyBuilder.ToMessageBody();

            // 3. Налаштовуємо "поштаря" (SmtpClient)
            using (var client = new SmtpClient())
            {
                // Підключаємось до SMTP-сервера Gmail
                // (587 - це стандартний порт для безпечної відправки)
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                // 4. Логінимось у вашу пошту
                // УВАГА: Для Gmail тут потрібен "Пароль додатка"
                await client.AuthenticateAsync("apuhlij66@gmail.com", "bearlox135798852Aa2)");

                // 5. Відправляємо
                _ = await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}