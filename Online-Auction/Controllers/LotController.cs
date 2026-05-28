using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;
using Online_Auction.Services;
using System.Globalization;
using System.Security.Claims;

namespace Online_Auction.Controllers
{
    [Authorize]
    public class LotController : Controller
    {
        private readonly LotService lotService;
        private readonly CategoryService categoryService;      
      
        public LotController(LotService lotService,
            CategoryService categoryService)
        {
            this.lotService = lotService;
            this.categoryService = categoryService;       
           
        }

        [HttpGet]
        [Route("/createlot")]
        public async Task<IActionResult> CreateLot() 
        {
            var categories = await categoryService.GetAllCategoriesAsync();       
            ViewBag.Categories = categories;

            return View("createlot");
        }

        [HttpPost]
        [Route("/createlot")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLot(IFormFile? imageFile)
        {
            var categories = await categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;

            var form = HttpContext.Request.Form;            

            var category = int.Parse(form["categoryId"]);           
            var title = form["title"].ToString().Trim();
            var description = form["descriptionLot"].ToString().Trim();
            var priceStr = form["startPrice"].ToString().Trim();
            var createdAt = form["createdAt"].ToString().Trim();
            var datetime = form["endDate"].ToString().Trim();

            if (!decimal.TryParse(priceStr, NumberStyles.Number,
                CultureInfo.InvariantCulture, out decimal startPrice))
            {
                ModelState.AddModelError("startPrice",
                    "Укажите корректную цену (например: 452.30)");
                return View("createlot");
            }

            if (startPrice <= 0)
                return BadRequest("Цена должна быть больше 0");

            if (string.IsNullOrEmpty(title) || title.Length < 3)
                return BadRequest("Название должно содержать минимум 3 символа");

            if (string.IsNullOrEmpty(description) || description.Length < 10)
                return BadRequest("Описание должно содержать минимум 10 символов");          

            // Получаем ID текущего пользователя
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Пользователь не авторизован");

            var cat = await categoryService.GetCategoryByIdAsync(category);

            // Обработка загруженного изображения
            // Значение по умолчанию
            string imageUrl = "/images/default-lot.jpg";  

            if (imageFile != null && imageFile.Length > 0)
            {
                // Генерируем уникальное имя
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                // Сохраняем файл на диск сервера
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Обновляем переменную новым значением
                imageUrl = $"/images/{fileName}"; 
            }

            var lot = new Lot
            {
                SellerId = int.Parse(userId),
                CategoryId = cat?.Id,
                Title = title,
                DescriptionLot = description,
                StartPrice = startPrice,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.Parse(createdAt),             
                EndDate = DateTime.Parse(datetime),  
                CurrentPrice = startPrice,       
                Status = LotStatus.Review
            };

            bool result = await lotService.AddLotAsync(lot);

            if (!result)
            {
                ModelState.AddModelError("", "Не удалось создать лот." +
                    " Попробуйте позже.");
                return View("~/Views/Lot/createlot.cshtml");
            }

            return Redirect("usaccount");
        }

        [HttpGet]
        [Route("/details/{id:int}")]
        public async Task<IActionResult> DetailesLot(int id)
        {
            var lot = await lotService.GetLotsByIdAsync(id); 

            if (lot == null) return NotFound();

            return View("detailslot", lot); 
        }

        // Страница торгов (один лот)
        [HttpGet]
        [Route("/trading/{id:int}")]
        public async Task<IActionResult> Trading(int id)
        {
            var lot = await lotService.GetLotsByIdAsync(id);

            if (lot == null) return NotFound();

            return View(lot);
        }

        // Форма редактирования
        [HttpGet]
        [Route("/lot/edit/{id:int}")]
        public async Task<IActionResult> EditLot(int id)
        {            
            var lot = await lotService.GetLotsByIdAsync(id);

            if (lot == null) return NotFound("Лот не найден или доступ запрещён");

            ViewBag.Categories = await categoryService.GetAllCategoriesAsync();
            return View("~/Views/Lot/editLot.cshtml", lot);
        }

        // Редактировать
        [HttpPost]
        [Route("/lot/edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLot(int id,
            Lot updatedLot, IFormFile? imageFile)
        {
            // Получаем ID текущего пользователя
            var userId = int.Parse(User
                .FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Валидация
            if (string.IsNullOrWhiteSpace(updatedLot.Title) ||
                updatedLot.Title.Length < 3)
            {
                ModelState.AddModelError("Title", 
                    "Название должно содержать минимум 3 символа");
            }

            if (string.IsNullOrWhiteSpace(updatedLot.DescriptionLot) ||
                updatedLot.DescriptionLot.Length < 10)
            {
                ModelState.AddModelError("DescriptionLot",
                    "Описание должно содержать минимум 10 символов");
            }

            if (updatedLot.StartPrice <= 0)
            {
                ModelState.AddModelError("StartPrice",
                    "Цена должна быть больше 0");
            }
      
            ViewBag.Categories = await categoryService.GetAllCategoriesAsync();            
            
            var result = await lotService.EditLotAsync(updatedLot, imageFile, userId);

            if (result)
                TempData["Success"] = "Лот успешно обновлён и отправлен на проверку";
            else
                TempData["Error"] = "Не удалось обновить лот";

            return View("~/Views/Lot/EditLot.cshtml", updatedLot);
        }

    }
}
