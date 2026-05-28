using System;
using System.Collections.Generic;

namespace Online_Auction.Models;

public partial class Auction
{
    public int Id { get; set; }

    public int LotId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string? StatusA { get; set; }

    public decimal? MinbidIncrement { get; set; }

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    public virtual Lot Lot { get; set; } = null!;

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
