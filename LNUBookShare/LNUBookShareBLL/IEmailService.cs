namespace LNUBookShareBLL
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string userEmail, string confirmationLink);
    }
}