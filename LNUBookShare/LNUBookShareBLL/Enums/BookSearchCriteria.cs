using System.ComponentModel;

namespace LNUBookShareBLL.Enums
{
    public enum BookSearchCriteria
    {
        [Description("Назва")]
        Title,
        [Description("Автор")]
        Author,
        [Description("ISBN")]
        ISBN,
        [Description("Категорія")]
        Category,
    }
}