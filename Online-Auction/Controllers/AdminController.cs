using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;
using Online_Auction.Services;
using System.Security.Claims;

namespace Online_Auction.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly LotService lotService;
        private readonly AuctionService auctionService;
        private readonly UserService userService;
        private readonly FeedbackService feedbackService;
        public AdminController(LotService lotService,
            AuctionService auctionService,
            UserService userService,
            FeedbackService feedbackService)
        {
            this.lotService = lotService;   
            this.auctionService = auctionService;
            this.userService = userService;
            this.feedbackService = feedbackService;
        }
        // все лоты у admin
        [HttpGet]
        [Route("/admin/pendingAllLots")]
        public async Task<IActionResult> LotsList()
        {
            var lots = await lotService.GetAllLotsAsync();
            ViewBag.Lots = lots;
            return View();
        }

        [HttpGet]
        [Route("/admin/pendingLot")]
        public async Task<IActionResult> PendingLot()
        {
            var lots = await lotService.GetLotsByStatusAsync(LotStatus.Review);
            ViewBag.Lots = lots;
            return View();
        }

        [HttpPost]
        [Route("/admin/lot/{id}/status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeLotStatus(int id, LotStatus status)
        {
            var lot = await lotService.GetLotsByIdAsync(id);
            if (lot == null) return NotFound();

            // Меняем статус
            lot.Status = status;

            // Если одобряем - создаём запись в Auction
            if (status == LotStatus.Active)
            {
                 lot.CurrentPrice = lot.StartPrice; // сброс цены при старте

                // Проверяем, нет ли уже аукциона
                if (lot.Auction == null)
                {
                    var auction = new Auction
                    {
                        LotId = lot.Id,
                        StartTime = DateTime.Now,
                        EndTime = lot.EndDate,
                        StatusA = "Active",
                        MinbidIncrement = 10.00m // можно вынести в настройки
                    };
                    bool auctionResult = await auctionService.AddAuctionAsync(auction);
                    if (!auctionResult)
                    {
                        Console.WriteLine("Не удалось создать аукцион");
                        return Redirect("pendingLot");
                    }
                }
            }
            bool lotResult = await lotService.UpdateLotAsync(lot);

            return Redirect("~/admin/pendingLot");
        }

        [HttpPost]
        [Route("/lot/{id}/status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLotStatus(int id, LotStatus status)
        {
            var lot = await lotService.GetLotsByIdAsync(id);
            if (lot == null) return NotFound();

            // Меняем статус
            lot.Status = status;

            bool lotResult = await lotService.UpdateLotAsync(lot);

            return Redirect("~/admin/pendingAllLots");
        }

        [HttpGet]
        [Route("/admin/userManagement")]
        public async Task<IActionResult> UserManagement()
        {
            var users = await userService.GetUsersAllAsync();
            ViewBag.Users = users;

            return View("userManagement");
        }

        [HttpPost]
        [Route("/admin/user/{userId}/status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId, string status)
        {
            // Админ не может забанить сам себя
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == currentUserId)
            {
                TempData["Error"] = "Нельзя изменить свой собственный статус";
                return Redirect("/admin/userManagement");
            }

            // Проверяем, что пользователь существует
            var user = await userService.GetUserByIdAsync(int.Parse(userId));
            if (user == null)
            {
                TempData["Error"] = "Пользователь не найден";
                return Redirect("/admin/userManagement");
            }

            var ban = false;
            if (status == "block") ban = true;          

            // Меняем статус
            bool result = await userService.ToggleBanStatusAsync(int.Parse(userId), ban);

            if (result)
            {
                TempData["Success"] =
                    $"Пользователь {user.FirstName} {user.LastName} {(ban ? "заблокирован" : "разблокирован")}";
            }
            else
            {
                TempData["Error"] =
                    "Не удалось изменить статус пользователя";
            }

            // Возвращаемся на страницу управления
            return Redirect("/admin/userManagement");
        }
      
        [HttpGet]
        [Route("/admin/adminMessages")]
        public async Task<IActionResult> AdminMessages()
        {
            var feedbacks = await feedbackService.GetAllFeedbacksAsync();

            // Создаём список: (обращение, найденный пользователь)
            var model = new List<(Feedback feedback, User user)>();

            foreach (var fb in feedbacks)
            {
                var user = await userService.FindByContactAsync(fb.SenderContact);
                model.Add((fb, user));
            }

            return View(model);
        }

    }
}
