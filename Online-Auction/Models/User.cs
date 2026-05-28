using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Online_Auction.Models;
public partial class User
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = null!;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = null!;

    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MaxLength(50)]
    public string LoginUs { get; set; } = null!;

    [Required, MinLength(8)]      
    public string PasswordHash { get; set; } = null!;

    [Required]
    public bool? IsBanned { get; set; }

    // счётчик неудачных попыток входа
    [Required]
    public int FailedLoginAttempts { get; set; } = 0;
    public int? RoleId { get; set; }

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    public virtual ICollection<Lot> Lots { get; set; } = new List<Lot>();

    public virtual ICollection<Purchase> PurchaseBuyers { get; set; } = new List<Purchase>();

    public virtual ICollection<Purchase> PurchaseSellers { get; set; } = new List<Purchase>();

    public virtual Role? Role { get; set; }
}
