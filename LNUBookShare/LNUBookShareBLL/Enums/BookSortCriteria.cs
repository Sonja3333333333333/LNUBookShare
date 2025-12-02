using System.ComponentModel;

namespace LNUBookShareBLL.Enums
{
    public enum BookSortCriteria
    {
        [Description("Назва")]
        Title,
        [Description("Автор")]
        Author,
        [Description("Рік")]
        Year,
        [Description("Категорія")]
        Category,
    }
}