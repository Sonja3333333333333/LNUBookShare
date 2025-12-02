namespace LNUBookShareBLL.DTOs
{
    public class LoginResultDto
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? FacultyName { get; set; }
    }
}