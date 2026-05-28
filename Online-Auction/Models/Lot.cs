using System;
using System.Collections.Generic;

namespace Online_Auction.Models;

public enum LotStatus
{
    NotActive = 1,    //Не активен
    Review = 2,       //На рассмотрении
    Active = 3,       //Активен
    Sold = 4,         //Продан
    Blocked = 5,      //Заблокирован
    Upblocked = 6     //Разблокирован
}
public partial class Lot
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? DescriptionLot { get; set; }

    public decimal StartPrice { get; set; }

    public decimal? CurrentPrice { get; set; } = null;

    public int SellerId { get; set; }

    public int? CategoryId { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public DateTime EndDate { get; set; } = default!;

    public string? ImageUrl { get; set; }
    public LotStatus Status { get; set; } = LotStatus.NotActive;

    public virtual Auction? Auction { get; set; }

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    public virtual Category? Category { get; set; }

    public virtual Purchase? Purchase { get; set; }

    public virtual User Seller { get; set; } = null!;
}
