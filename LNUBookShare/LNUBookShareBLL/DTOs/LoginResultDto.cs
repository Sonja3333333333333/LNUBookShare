namespace LNUBookShareBLL.DTOs
{
    /// <summary>
    /// Об'єкт, що повертається у UI при успішному вході в систему.
    /// </summary>
    public class LoginResultDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string FacultyName { get; set; } // UI-частині потрібна назва, а не ID

        // TODO: Можна додати шлях до аватара, якщо він потрібен одразу
        // public string? AvatarPath { get; set; } 
    }
}