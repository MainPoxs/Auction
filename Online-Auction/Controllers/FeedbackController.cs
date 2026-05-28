using Microsoft.AspNetCore.Mvc;
using Online_Auction.Models;
using Online_Auction.Services;

namespace Online_Auction.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly FeedbackService feedbackService;
        public FeedbackController(FeedbackService feedbackService)
        {
            this.feedbackService = feedbackService;
        }
        // Страница с формой
        [HttpGet]
        [Route("/contact-admin")]
        public IActionResult WriteToAdmin()
        {
            return View();
        }

        // Обработка отправки
        [HttpPost]
        [Route("/contact-admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendFeedback(Feedback model)
        {
            if (string.IsNullOrWhiteSpace(model.SenderContact) ||
                string.IsNullOrWhiteSpace(model.MessageText))
            {
                TempData["Error"] = "Заполните все поля";
                return View(model);
            }

            model = new Feedback
            {
                SenderContact = model.SenderContact,
                Subject = model.Subject,
                MessageText = model.MessageText,            
            };

            await feedbackService.SendFeedbackAsync(model);

            // Перенаправляем на страницу успеха
            return RedirectToAction("FeedbackSuccess");
        }

        [HttpGet]
        [Route("/feedback-success")]
        public IActionResult FeedbackSuccess()
        {
            return View();
        }

    }
}
