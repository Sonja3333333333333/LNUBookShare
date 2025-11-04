namespace LNUBookShareBLL.DTOs
{
    /// <summary>
    /// Універсальний DTO для результатів з пагінацією.
    /// </summary>
    public class PaginatedResultDto<T>
    {
        /// <summary>
        /// Елементи на поточній сторінці.
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// Загальна кількість знайдених елементів (для розрахунку сторінок).
        /// </summary>
        public int TotalCount { get; set; }
    }
}