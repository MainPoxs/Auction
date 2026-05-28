using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Auction.Services;
using System.Security.Claims;

namespace Online_Auction.Controllers
{
    [Authorize]
    public class TradingController : Controller
    {
        private readonly LotService _lotService;
        private readonly AuctionService _auctionService;

        public TradingController(LotService lotService,
            AuctionService auctionService)
        {
            _lotService = lotService;
            _auctionService = auctionService;
        }     

        [HttpGet]
        [Route("/tradingRoom/{lotId:int}")]
        public async Task<IActionResult> TradingRoom(int lotId)
        {
            var lot = await _lotService.GetLotsByIdAsync(lotId);

            if (lot == null || lot.Auction == null)
                return NotFound("Лот не найден");
      
            if (lot.Auction.StatusA != "Active" ||
                lot.Auction.EndTime <= DateTime.Now)
                return RedirectToAction("GetWinner", "Auction", new { lotId });
       
            ViewBag.Lot = lot;         
            ViewBag.CurrentPrice = lot.CurrentPrice;

            return View();
        }
    }
}
