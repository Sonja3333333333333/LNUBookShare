namespace LNUBookShareBLL.DTOs
{

    public class ProfileEditDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int FacultyId { get; set; }
        public string ProfileImageUrl { get; set; } = string.Empty;
        // Ми не повертаємо email, оскільки його не можна змінювати.
    }
}