using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;

namespace Online_Auction.Services;

public partial class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions<AuctionDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Feedback> Feedbacks { get; set; }
    public virtual DbSet<Auction> Auctions { get; set; }

    public virtual DbSet<Bid> Bids { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Lot> Lots { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("auctions_pkey");

            entity.ToTable("auctions");

            entity.HasIndex(e => e.LotId, "auctions_lot_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_time");
            entity.Property(e => e.LotId).HasColumnName("lot_id");
            entity.Property(e => e.MinbidIncrement)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("100.00")
                .HasColumnName("minbid_increment");
            entity.Property(e => e.StartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_time");
            entity.Property(e => e.StatusA)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Scheduled'::character varying")
                .HasColumnName("status_a");

            entity.HasOne(d => d.Lot).WithOne(p => p.Auction)
                .HasForeignKey<Auction>(d => d.LotId)
                .HasConstraintName("auctions_lot_id_fkey");
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bids_pkey");

            entity.ToTable("bids");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.AuctionId).HasColumnName("auction_id");
            entity.Property(e => e.BidderId).HasColumnName("bidder_id");
            entity.Property(e => e.LotId).HasColumnName("lot_id");

            entity.HasOne(d => d.Auction).WithMany(p => p.Bids)
                .HasForeignKey(d => d.AuctionId)
                .HasConstraintName("bids_auction_id_fkey");

            entity.HasOne(d => d.Bidder).WithMany(p => p.Bids)
                .HasForeignKey(d => d.BidderId)
                .HasConstraintName("bids_bidder_id_fkey");

            entity.HasOne(d => d.Lot).WithMany(p => p.Bids)
                .HasForeignKey(d => d.LotId)
                .HasConstraintName("bids_lot_id_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NameC)
                .HasMaxLength(100)
                .HasColumnName("name_c");
        });

        modelBuilder.Entity<Lot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lots_pkey");

            entity.ToTable("lots");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentPrice)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("current_price");
            entity.Property(e => e.DescriptionLot).HasColumnName("description_lot");
            entity.Property(e => e.EndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_date");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.StartPrice)
                .HasPrecision(12, 2)
                .HasColumnName("start_price");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Category).WithMany(p => p.Lots)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lots_category_id_fkey");

            entity.HasOne(d => d.Seller).WithMany(p => p.Lots)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("lots_seller_id_fkey");
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purchases_pkey");

            entity.ToTable("purchases");

            entity.HasIndex(e => e.LotId, "purchases_lot_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuctionId).HasColumnName("auction_id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.FinalPrice)
                .HasPrecision(12, 2)
                .HasColumnName("final_price");
            entity.Property(e => e.LotId).HasColumnName("lot_id");
            entity.Property(e => e.PurchaseDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("purchase_date");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.StatusP)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status_p");

            entity.HasOne(d => d.Auction).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.AuctionId)
                .HasConstraintName("purchases_auction_id_fkey");

            entity.HasOne(d => d.Buyer).WithMany(p => p.PurchaseBuyers)
                .HasForeignKey(d => d.BuyerId)
                .HasConstraintName("purchases_buyer_id_fkey");

            entity.HasOne(d => d.Lot).WithOne(p => p.Purchase)
                .HasForeignKey<Purchase>(d => d.LotId)
                .HasConstraintName("purchases_lot_id_fkey");

            entity.HasOne(d => d.Seller).WithMany(p => p.PurchaseSellers)
                .HasForeignKey(d => d.SellerId)
                .HasConstraintName("purchases_seller_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(20)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.RoleId, "fki_users_role_id_fkey");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.LoginUs, "users_login_us_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.IsBanned)
                .HasDefaultValue(false)
                .HasColumnName("is_banned");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.LoginUs)
                .HasMaxLength(50)
                .HasColumnName("login_us");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("users_role_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
