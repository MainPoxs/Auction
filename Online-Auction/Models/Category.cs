using System;
using System.Collections.Generic;

namespace Online_Auction.Models;

public partial class Category
{
    public int Id { get; set; }

    public string NameC { get; set; } = null!;

    public virtual ICollection<Lot> Lots { get; set; } = new List<Lot>();
}
