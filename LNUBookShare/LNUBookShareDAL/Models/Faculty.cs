using System;
using System.Collections.Generic;

namespace LNUBookShareDAL.Models;

public partial class Faculty
{
    public int FacultyId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; } = new List<User>();
}
