namespace LNUBookShareBLL.DTOs
{
    
    public class FavoriteBookCardDto
    {
        public int BookId { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public int? Year { get; set; }
        public string? Status { get; set; }
        public string? CoverPath { get; set; }

        public int OwnerId { get; set; }

        public string? OwnerFullName { get; set; }
        
    }
}
