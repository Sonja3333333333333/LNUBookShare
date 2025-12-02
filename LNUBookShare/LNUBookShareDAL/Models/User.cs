namespace LNUBookShareDAL.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int FacultyId { get; set; }

    public int? AvatarId { get; set; }

    public bool IsEmailConfirmed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Image? Avatar { get; set; }

    public virtual ICollection<Book> Books { get; } = new List<Book>();

    public virtual Emailconfirmation? Emailconfirmation { get; set; }

    public virtual Faculty Faculty { get; set; } = null!;

    public virtual ICollection<Favorite> Favorites { get; } = new List<Favorite>();
}
