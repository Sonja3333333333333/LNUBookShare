namespace LNUBookShareDAL.Models;

public partial class Emailconfirmation
{
    public int ConfirmationId { get; set; }

    public int UserId { get; set; }

    public string ConfirmationToken { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual User User { get; set; } = null!;
}
