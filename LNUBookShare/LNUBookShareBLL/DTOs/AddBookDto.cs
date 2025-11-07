namespace LNUBookShareBLL.DTOs
{
    /// <summary>
    /// DTO, що містить дані для створення нової книги.
    /// </summary>
    public class AddBookDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string? Isbn { get; set; }
        public int? Year { get; set; }
        public string? Publisher { get; set; }
        public string? Language { get; set; }
        public int CategoryId { get; set; }

        // TODO: Додати CoverId або логіку завантаження файлу
        // public int? CoverId { get; set; } 
    }
}