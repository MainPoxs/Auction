
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class BidService
    {
        private readonly AuctionDbContext db;
        public BidService(AuctionDbContext db)
        {
            this.db = db;        
        }
        public async Task<bool> AddBidAsync(Bid bid)
        {
            try
            {
                await db.Bids.AddAsync(bid);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Выводим ВСЮ цепочку ошибок
                Console.WriteLine($"Ошибка AddBidAsync: {ex.Message}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");

                    // Если есть ещё вложенные исключения
                    if (ex.InnerException.InnerException != null)
                    {
                        Console.WriteLine($"InnerInner: {ex.InnerException.InnerException.Message}");
                    }
                }

                // StackTrace для точного места ошибки
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                return false;
            }
        }
       
    }
}
