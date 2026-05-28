using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class FeedbackService
    {
        private readonly AuctionDbContext db;
        public FeedbackService(AuctionDbContext context)
        {
            db = context;
        }
        public async Task<bool> SendFeedbackAsync(Feedback feedback)
        {
            await db.Feedbacks.AddAsync(feedback);
            await db.SaveChangesAsync();
            return true;
        }
        public async Task<List<Feedback>> GetAllFeedbacksAsync()
        {
            return await db.Feedbacks
                .ToListAsync();
        }
    }
}
