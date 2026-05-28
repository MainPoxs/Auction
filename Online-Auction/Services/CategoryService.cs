using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class CategoryService
    {
        private readonly AuctionDbContext db;
        public CategoryService(AuctionDbContext context)
        {
            db = context;
        }
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await db.Categories.ToListAsync();
        }
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await db.Categories.FindAsync(id);
        }
    }
}
