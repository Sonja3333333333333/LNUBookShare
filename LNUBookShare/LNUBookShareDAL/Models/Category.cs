namespace LNUBookShareDAL.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Book> Books { get; } = new List<Book>();
}
