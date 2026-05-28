using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;
using Online_Auction.Services;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Online_Auction.Controllers
{
    [Authorize]
    public class AuctionController : Controller
    {
        private readonly AuctionService auctionService;
        private readonly LotService lotService;
        private readonly CategoryService categoryService;
        private readonly PurchaseService purchaseService;
        
        public AuctionController(AuctionService auctionService,
            LotService lotService, PurchaseService purchaseService,
            CategoryService categoryService)
        {
            this.auctionService = auctionService;           
            this.lotService = lotService;
            this.purchaseService = purchaseService;
            this.categoryService = categoryService;
        }
     
        [HttpGet]
        [Route("/allresults")]
        public async Task<IActionResult> GetAllresults(LotFilter filter = LotFilter.Active,
            int? categoryId = null)
        {
            // 1. Обновляем просроченные аукционы
            await auctionService.UpdateExpiredAuctionsAsync();

            // 2. Получаем лоты по фильтрам
            var lots = await lotService.GetLotsByFilterAsync(filter, categoryId);

            // 3. Один запрос категорий (без дублирования!)
            var categories = await categoryService.GetAllCategoriesAsync();

            // 4. Передаём данные в View
            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.Categories = categories;

            // 5. 🔹 ПЕРЕДАЁМ МОДЕЛЬ (список лотов)
            return View("~/Views/Shared/allresults.cshtml", lots);
        }

        [HttpGet]
        [Route("/auction/winner/{lotId:int}")]
        public async Task<IActionResult> GetWinner(int lotId)
        {
            // Получаем лот с данными
            var lot = await lotService.GetLotsByIdAsync(lotId);

            if (lot == null || lot.Auction == null)
                return NotFound("Лот не найден");

            // Получаем победителя для этого лота
            var purchase = await purchaseService
                .GetPurchaseByLotIdAsync(lotId);

            // Передаём данные в View
            ViewBag.Purchase = purchase;
            return View("~/Views/Auction/winner.cshtml", lot.Auction);
        }

    }
}
