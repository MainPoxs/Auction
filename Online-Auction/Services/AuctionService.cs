using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class AuctionService
    {
        private readonly AuctionDbContext db;
        private readonly PurchaseService purchaseService;

        public AuctionService(AuctionDbContext db,
            PurchaseService purchaseService)
        {
            this.db = db;
            this.purchaseService = purchaseService;
        }
        public async Task<bool> AddAuctionAsync(Auction auction)
        {
            try
            {
                await db.Auctions.AddAsync(auction);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Аукцион не создан: {ex.Message}");                
                return false;
            }
        }      
        public async Task UpdateExpiredAuctionsAsync()
        {            
            var expiredAuctions = await db.Auctions
                .Include(a => a.Lot)
                .Include(a => a.Bids)
                .Where(a => a.StatusA == "Active" && a.EndTime <= DateTime.Now)
                .ToListAsync();

            foreach (var auction in expiredAuctions)
            {
                auction.StatusA = "Ended";

                if (auction.Lot != null)
                {
                    var winningBid = auction.Bids
                        .OrderByDescending(b => b.Amount)
                        .FirstOrDefault();

                    // Если есть победитель, то создаем покупку
                    if (winningBid != null)
                    {
                        await purchaseService.CreatePurchaseFromWinBidAsync(
                            winningBid,
                            auction.Lot,
                            auction
                        );
                    }
                    else
                    {
                        auction.Lot.Status = LotStatus.NotActive;
                    }
                }
            }

            if (expiredAuctions.Any())
                await db.SaveChangesAsync();
        }

        public async Task<List<Auction>> GetEndedAuctionsAsync()
        {
            var expiredAuctions = await db.Auctions
                .Include(a => a.Lot)
                    .ThenInclude(l => l.Purchase)
                .Include(a => a.Bids)
                .Where(a => a.StatusA == "Ended")
                .OrderByDescending(a => a.EndTime)
                .ToListAsync();

            return expiredAuctions;
        }

        public async Task<Auction> GetAuctionByIdAsync(int id)
        {
            var auction = await db.Auctions
                .Include(a => a.Lot)
                .ThenInclude(l => l.Category)
                .Include(a => a.Lot)
                .ThenInclude(l => l.Seller)
                .Include(a => a.Purchases)  
                .ThenInclude(p => p.Buyer)                
                .FirstOrDefaultAsync(a => a.Id == id);

            return auction;
        }

        //Получение последних ставок
        public async Task<List<Bid>> GetRecentBidsAsync(int lotId, int count = 10)
        {
            return await db.Bids
                .Include(b => b.Bidder)
                .Where(b => b.LotId == lotId)
                .OrderByDescending(b => b.BidTime)
                .Take(count)
                .ToListAsync();
        }
    }
}
