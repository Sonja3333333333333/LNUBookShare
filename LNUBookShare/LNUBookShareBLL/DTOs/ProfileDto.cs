namespace LNUBookShareBLL.DTOs
{
    public class ProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{this.FirstName} {this.LastName}";
        public string? Email { get; set; }

        public string? FacultyName { get; set; }
        public string? AvatarPath { get; set; }
        public List<OwnedBookDto>? OwnedBooks { get; set; }
    }
}
