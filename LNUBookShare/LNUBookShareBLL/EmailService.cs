using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;

using System.Threading.Tasks;

// Переконайтеся, що namespace LNUBookShareBLL
namespace LNUBookShareBLL
{
    public class EmailService
    {
        // Асинхронний метод для відправки листа
        public async Task SendConfirmationEmailAsync(string userEmail, string confirmationLink)
        {
            var message = new MimeMessage();

            // !! ЗАМІНІТЬ ЦЕ НА ВАШУ ПОШТУ ТА ПАРОЛЬ ДОДАТКА !!
            message.From.Add(new MailboxAddress("LNU Book Share", "apuhlij66@gmail.com"));
            message.To.Add(new MailboxAddress("Новий Користувач", userEmail));
            message.Subject = "Підтвердження реєстрації LNU Book Share";

            // Створюємо HTML-тіло листа
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

            using (var client = new SmtpClient())
            {
                // Налаштування для Gmail
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                // !! ПАРОЛЬ ДОДАТКА (APP PASSWORD) ВІД GOOGLE !!
                await client.AuthenticateAsync("apuhlij66@gmail.com", "ljgsgvptewwucrpy");

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}