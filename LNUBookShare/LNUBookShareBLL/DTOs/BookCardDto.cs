namespace LNUBookShareBLL.DTOs
{
    /// <summary>
    /// DTO для відображення картки книги у каталозі. Список книг
    /// </summary>
    public class BookCardDto
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int? Year { get; set; }
        public string OwnerFullName { get; set; }
        public string? CoverPath { get; set; }
        public int OwnerId { get; set; } // додано
        /// <summary>
        /// Чи вподобав цю книгу поточний користувач (для іконки сердечка).
        /// </summary>
        public bool IsFavoritedByCurrentUser { get; set; }
    }
}