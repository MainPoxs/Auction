using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Auction.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Online_Auction.Controllers
{
    public class UserController : Controller
    {
        private readonly LotService lotService;
        private readonly PurchaseService purchaseService;
        private readonly AuctionService auctionService;
        public UserController(LotService lotService,
            PurchaseService purchaseService,
            AuctionService auctionService)
        {
            this.lotService = lotService;
            this.purchaseService = purchaseService;
            this.auctionService = auctionService;
        }

        [Authorize(Policy ="UserOnly")]
        [Route("/usaccount")]
        public async Task<IActionResult> GetUserAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Redirect("login");
            }

            var lots = await lotService.GetLotsByUserIdAsync(int.Parse(userId));
            return View("usaccount", lots);
        }

        [Authorize(Policy = "AdminOnly")]
        [Route("/adminaccount")]
        public IActionResult GetAdminAccount()
        {
            return View("adminaccount");
        }       

    }
}
