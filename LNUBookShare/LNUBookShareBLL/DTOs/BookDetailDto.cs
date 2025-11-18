namespace LNUBookShareBLL.DTOs
{

    public class BookDetailsDto
    {
        public int BookId { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Isbn { get; set; }
        public int? Year { get; set; }
        public string? Publisher { get; set; }
        public string? Language { get; set; }
        public string? Status { get; set; }
        public string? CoverPath { get; set; }

        public string? CategoryName { get; set; }
        public string? OwnerFullName { get; set; }
        public string? OwnerEmail { get; set; }

        public int OwnerId { get; set; }

        public bool IsFavoritedByCurrentUser { get; set; }
    }
}
