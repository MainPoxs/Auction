using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;
using Online_Auction.Services;
using System.Net.NetworkInformation;
using System.Security.Claims;

namespace Online_Auction.Controllers
{
    [Authorize]
    public class BidController : Controller
    {
        private readonly LotService lotService;
        private readonly BidService bidService;
        private readonly IHubContext<ChatHub> hubContext;
        public BidController(LotService lotService,
            BidService bidService,
            IHubContext<ChatHub> hubContext)
        {
            this.lotService = lotService;
            this.bidService = bidService;
            this.hubContext = hubContext;
        }

        [HttpPost]
        [Route("/placeBid")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(int lotId, decimal amount)
        {
            // Находим лот с аукционом
            var lot = await lotService.GetLotWithAuctionAsync(lotId);

            if (lot == null)
            {
                TempData["Error"] = "Лот не найден";
                return Redirect($"/trading/{lotId}");
            }

            // Получаем ID текущего пользователя
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Redirect("login");
            }

            // Проверки статуса и времени
            if (lot.Status != LotStatus.Active)
            {
                TempData["Error"] = "Торги по этому лоту не активны";
                return Redirect($"/trading/{lotId}");
            }

            if (lot.EndDate <= DateTime.Now)
            {
                TempData["Error"] = "Время торгов истекло";
                return Redirect($"/trading/{lotId}");
            }
            // Проверка минимальной ставки
            var currentPrice = lot.CurrentPrice ?? lot.StartPrice;
            var minIncrement = lot.Auction?.MinbidIncrement ?? 10.00m;
            var minBid = currentPrice + minIncrement;

            if (amount <= minBid)
            {
                TempData["Error"] = $"Ставка слишком мала!";
                return Redirect($"/trading/{lotId}");
            }
            // Создаём ставку
            var bid = new Bid
            {
                LotId = lotId,
                AuctionId = lot.Auction.Id, 
                BidderId = int.Parse(userId),
                Amount = amount,
                BidTime = DateTime.Now
            };

            bool bidResult = await bidService.AddBidAsync(bid);
            if (!bidResult)
            {
                TempData["Error"] = "Не удалось создать ставку";
                return Redirect($"/trading/{lotId}");
            }

            // Обновляем цену лота
            lot.CurrentPrice = amount;
            bool lotResult = await lotService.UpdateLotAsync(lot);

            var userName = User.Identity?.Name ?? "Аноним";

            // Рассылаем обновление через SignalR
            await hubContext.Clients.Group($"lot-{lotId}")
                .SendAsync("PriceUpdated", new
            {
                LotId = lotId,
                newPrice = amount,
                UserName = userName,
                Time = DateTime.Now
            });

            TempData["Success"] = $"Ставка принята!";

            return Redirect($"/trading/{lotId}");
        }
    }
}

