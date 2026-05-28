using System;
using System.Collections.Generic;

namespace Online_Auction.Models;

public partial class Purchase
{
    public int Id { get; set; }

    public int LotId { get; set; }

    public int AuctionId { get; set; }

    public int BuyerId { get; set; }

    public int SellerId { get; set; }

    public decimal? FinalPrice { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public string? StatusP { get; set; }

    public virtual Auction Auction { get; set; } = null!;

    public virtual User Buyer { get; set; } = null!;

    public virtual Lot Lot { get; set; } = null!;

    public virtual User Seller { get; set; } = null!;
}
