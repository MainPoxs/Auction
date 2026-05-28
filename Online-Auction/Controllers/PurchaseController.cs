using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Auction.Services;
using System.Security.Claims;

namespace Online_Auction.Controllers
{
    [Authorize]
    public class PurchaseController : Controller
    {
        private readonly AuctionService auctionService;
        private readonly PurchaseService purchaseService;
        public PurchaseController(AuctionService auctionService,
            PurchaseService purchaseService) 
        {
            this.auctionService = auctionService;
            this.purchaseService = purchaseService;
        }
     
        [HttpGet]
        [Route("/mypurchases")]
        public async Task<IActionResult> MyPurchases()
        {
            await auctionService.UpdateExpiredAuctionsAsync();
            // Получаем ID текущего пользователя
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Получаем покупки где пользователь — покупатель И статус "выиграл"
            var purchases = await purchaseService
                .GetPurchasesByBuyerIdAsync(int.Parse(userId));

            return View("MyPurchases", purchases);
        }

        [HttpGet]
        [Route("/pay/{lotId:int}")]
        public async Task<IActionResult> Pay(int lotId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Получаем лот, чтобы показать его название и цену в форме
            var lot = await purchaseService.GetLotForPaymentAsync(lotId, int.Parse(userId));

            if (lot == null)
                return NotFound("Лот не найден или вы не являетесь его покупателем");

            return View("~/Views/Auction/pay.cshtml", lot); 
        }

        // Обработка оплаты
        [HttpPost]
        [Route("/success")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            var form = HttpContext.Request.Form;

            // Валидация
            var cardNum = form["cardNumber"].ToString().Trim();
            var cardCvv = form["cardCvv"].ToString().Trim();
            var lotId = form["lotId"].ToString().Trim();

            if (cardNum.Length != 16 || !cardNum.All(char.IsDigit))
            {
                TempData["Error"] = "Номер карты должен содержать 16 цифр";
                return RedirectToAction("Pay", new { lotId = lotId }); 
            }

            if (cardCvv.Length != 3 || !cardCvv.All(char.IsDigit))
            {
                TempData["Error"] = "CVV должен содержать 3 цифры";
                return RedirectToAction("Pay", new { lotId = lotId }); 
            }

            // Обновляем статус в БД
            var success = await purchaseService
                .MarkPurchaseAsPaidAsync(int.Parse(lotId), int.Parse(userIdStr));

            if (!success)
            {
                TempData["Error"] = "Лот не найден или уже оплачен";
                return RedirectToAction("Pay", new { lotId = lotId }); 
            }

            return RedirectToAction("GetSuccess");   
        }

        [HttpGet]
        [Route("/success")]
        public async Task<IActionResult> GetSuccess()
        {  
            return View("~/Views/Shared/success.cshtml");
        }

        [HttpGet]  
        [Route("/qrPayment/{lotId:int}")]
        public async Task<IActionResult> ConfirmPaymentByQR(int lotId)
        {
            // Получаем ID текущего пользователя
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            var buyerId = int.Parse(userIdStr);

            // Получаем Purchase
            var purchase = await purchaseService
                .GetPurchaseByLotIdAsync(lotId);

            // Проверяем владельца и статус
            if (purchase == null || purchase.BuyerId != buyerId ||
                purchase.StatusP == "Paid")
            {
                TempData["Error"] = "Лот не найден или уже оплачен";
                return RedirectToAction("Pay", new { lotId = lotId });
            }

            return View("~/Views/Shared/qrPayment.cshtml", purchase);

        }

        [HttpPost]
        [Route("/pay/confirm/{lotId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayByQR(int lotId)
        {
            // Получаем ID текущего пользователя
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            var buyerId = int.Parse(userIdStr);
            
            var success = await purchaseService.MarkPurchaseAsPaidAsync(lotId, buyerId);

            // Обрабатываем результат
            if (!success)
            {
                TempData["Error"] = "Лот не найден или уже оплачен";
                return RedirectToAction("Pay", new { lotId = lotId });
            }

            TempData["Success"] = "плата подтверждена!";
            return Redirect("/mypurchases");
        }

    }
}
