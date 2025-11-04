namespace LNUBookShareBLL.DTOs
{
    // DTO для картки книги у списку "Мої книги"
    public class OwnedBookDto
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int? Year { get; set; }
        public string Status { get; set; } // "available" або "issued"
        public string? CoverPath { get; set; }
    }
}
