using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LNUBookShareDAL.Models;



public partial class Image
{
    public int ImageId { get; set; }

    public string ImagePath { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    [Column("image_type")]
    public string ImageType { get; set; } = null!;

    public virtual ICollection<Book> Books { get; } = new List<Book>();

    public virtual ICollection<User> Users { get; } = new List<User>();
}
