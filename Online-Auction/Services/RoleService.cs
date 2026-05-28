using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class RoleService
    {
        private readonly AuctionDbContext db;
        public RoleService(AuctionDbContext context)
        {
            db = context;
        }
        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await db.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
        }

    }
}
