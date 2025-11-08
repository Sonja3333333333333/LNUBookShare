namespace LNUBookShareBLL.DTOs
{
    /// <summary>
    /// DTO, що містить дані для заповнення форми редагування книги.
    /// </summary>
    public class BookEditDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string? Isbn { get; set; }
        public int? Year { get; set; }
        public string? Publisher { get; set; }
        public string? Language { get; set; }
        public int CategoryId { get; set; }
        public string Status { get; set; } // "available" або "issued"
    }
}