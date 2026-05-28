using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class PurchaseService
    {
        private readonly AuctionDbContext db;

        public PurchaseService (AuctionDbContext db)
        {
            this.db = db;
        }
        public async Task<List<Purchase>> GetPurchasesByBuyerIdAsync(int buyerId)
        {
            return await db.Purchases
                .Include(p => p.Lot)
                .Include(p => p.Seller)
                .Include(p => p.Auction)
                .Where(p => p.BuyerId == buyerId)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }
        // Создание покупки на основе выигрышной ставки
        public async Task<Purchase> CreatePurchaseFromWinBidAsync
            (Bid winBid, Lot lot, Auction auction)
        {
            try
            {
                // Проверяем, не создана ли уже покупка для этого лота
                var existingPurchase = await db.Purchases
                    .FirstOrDefaultAsync(p => p.LotId == lot.Id &&
                    p.AuctionId == auction.Id);

                if (existingPurchase != null)
                {
                    Console.WriteLine($"Покупка для лота {lot.Id} уже существует");
                    return existingPurchase;
                }

                // Создаем новую покупку
                var purchase = new Purchase
                {
                    LotId = lot.Id,
                    AuctionId = auction.Id,
                    BuyerId = winBid.BidderId,
                    SellerId = lot.SellerId,
                    FinalPrice = winBid.Amount,
                    PurchaseDate = DateTime.Now,
                    StatusP = "NoPaid"
                };

                await db.Purchases.AddAsync(purchase);

                // Обновляем статус лота
                lot.Status = LotStatus.Sold;
                lot.CurrentPrice = winBid.Amount;

                await db.SaveChangesAsync();

                return purchase;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании покупки: {ex.Message}");
                throw;
            }
        }
        public async Task<Lot?> GetLotForPaymentAsync(int lotId, int buyerId)
        {
            return await db.Purchases
                .Include(p => p.Lot)
                .Where(p => p.LotId == lotId &&
                p.BuyerId == buyerId && p.StatusP == "NoPaid")
                .Select(p => p.Lot)
                .FirstOrDefaultAsync();
        }
        public async Task<bool> MarkPurchaseAsPaidAsync(int lotId, int buyerId)
        {
            var purchase = await db.Purchases
                .FirstOrDefaultAsync(p =>
                    p.LotId == lotId &&
                    p.BuyerId == buyerId &&
                    p.StatusP == "NoPaid");

            if (purchase == null)
                return false; 

            // Обновляем статус и дату покупки
            purchase.StatusP = "Paid";
            purchase.PurchaseDate = DateTime.Now;

            await db.SaveChangesAsync();
            return true;
        }
        //Получаем победителя для конкретного лота
        public async Task<Purchase?> GetPurchaseByLotIdAsync(int lotId)
        {
            return await db.Purchases
                .Include(p => p.Buyer)   
                .Include(p => p.Seller) 
                .Include(p => p.Lot)      
                .Include(p => p.Auction)  
                .FirstOrDefaultAsync(p => p.LotId == lotId);
        }
    }
}
