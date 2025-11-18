namespace LNUBookShareBLL.DTOs
{

    public class PaginatedResultDto<T>
    {
        public List<T>? Items { get; set; }

       
        public int TotalCount { get; set; }
    }
}