using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Auction.Models;

public partial class Bid
{
    public int Id { get; set; }

    public int LotId { get; set; }

    public int AuctionId { get; set; }

    public int BidderId { get; set; }

    public decimal Amount { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? BidTime { get; set; } = DateTime.Now;

    public virtual Auction Auction { get; set; } = null!;

    public virtual User Bidder { get; set; } = null!;

    public virtual Lot Lot { get; set; } = null!;
}
