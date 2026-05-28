using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public enum LotFilter
    {
        Active,      // Активные 
        Completed,   // Завершённые 
        All          // Все лоты
    }
    public class LotService
    {
        private readonly AuctionDbContext db;
        public LotService(AuctionDbContext context)
        {
            db = context;
        }
        public async Task<bool> AddLotAsync(Lot lot)
        {
            try
            {
                await db.Lots.AddAsync(lot);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception ex) 
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return false;
            }
        }

        //Активные лоты
        public async Task<List<Lot>> GetActiveLotsAsync(int limit = 20)
        {
            return await db.Lots
                .Include(l => l.Seller)
                .Include(l => l.Category)
                .Include(l => l.Auction)
                .Where(l => l.Status == LotStatus.Active
                         && l.EndDate > DateTime.Now
                         && l.Auction != null)
                .OrderByDescending(l => l.EndDate)
                .Take(limit)
                .ToListAsync();
        }

        //Завершённые лоты
        public async Task<List<Lot>> GetCompletedLotsAsync(int limit = 20)
        {
            return await db.Lots
                .Include(l => l.Seller)
                .Include(l => l.Category)
                .Include(l => l.Auction)
                .Where(l => l.EndDate <= DateTime.Now
                         || l.Status == LotStatus.Sold)
                .OrderByDescending(l => l.EndDate)
                .Take(limit)
                .ToListAsync();
        }

        // Все лоты 
        public async Task<List<Lot>> GetAllLotsAsync(int limit = 20)
        {
            return await db.Lots
                .Include(l => l.Seller)
                .Include(l => l.Category)
                .Include(l => l.Auction)
                .OrderByDescending(l => l.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        // Лоты для главной страницы (с фильтрацией)
        public async Task<List<Lot>> GetLotsForHomeAsync(int limit = 4)
        {
            return await db.Lots
                .Include(l => l.Seller)
                .Include(l => l.Category)
                .Include(l => l.Auction)
                .Where(l => l.Status != LotStatus.Blocked
                         && l.Status != LotStatus.Review) 
                .OrderByDescending(l => l.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        // Список лотов на модерации
        public async Task<List<Lot>> GetLotsByStatusAsync(LotStatus status)
        {
            var lots = await db.Lots
            .Include(l => l.Seller)
            .Include(l => l.Category)
            .Where(l => l.Status == status)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

            return lots;
        }

        // Поиск лота по Id
        public async Task<Lot> GetLotsByIdAsync(int id)
        {
            var lot = await db.Lots
                .Include(l => l.Seller)
                .Include(l => l.Category)
                .Include(l => l.Auction)
                .Include(l => l.Bids)
                .ThenInclude(b => b.Bidder) 
                .FirstOrDefaultAsync(l => l.Id == id);

            return lot;
        }

        public async Task<bool> UpdateLotAsync(Lot lot)
        {
            try
            {
                db.Lots.Update(lot); 
                await db.SaveChangesAsync();  
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message,
                    "Ошибка при обновлении лота {LotId}");
                return false;
            }
        }
        public async Task<Lot> GetLotWithAuctionAsync(int lotId)
        {
            var lot = await db.Lots
                 .Include(l => l.Auction)
                 .FirstOrDefaultAsync(l => l.Id == lotId);

            return lot;
        }
        public async Task<List<Lot>> GetLotsByUserIdAsync(int userId)
        {
            return await db.Lots
                .Include(l => l.Seller)
                .Include(l => l.Category)
                .Include(l => l.Auction)
                .Include(l => l.Bids)
                .Where(l => l.SellerId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }  

        // Фильтр по статусу и категории
        public async Task<List<Lot>> GetLotsByFilterAsync
            (LotFilter filter, int? categoryId = null)
        {
            var query = db.Lots
                .Include(l => l.Seller)
                .Include(l => l.Category)
                .Include(l => l.Auction)
                .AsQueryable();

            // Фильтр по категории (если выбрано)
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(l => l.CategoryId == categoryId);
            }

            var now = DateTime.Now;

            // Фильтр по статусу
            switch (filter)
            {
                case LotFilter.Active:
                    return await query
                        .Where(l => l.Status == LotStatus.Active
                                 && l.EndDate > now
                                 && l.Auction != null)
                        .OrderByDescending(l => l.EndDate)
                        .Take(20)
                        .ToListAsync();

                case LotFilter.Completed:
                    return await query
                        .Where(l => l.EndDate <= now
                                 || l.Status == LotStatus.Sold
                                 || l.Status == LotStatus.Blocked)
                        .OrderByDescending(l => l.EndDate)
                        .Take(20)
                        .ToListAsync();

                case LotFilter.All:
                default:
                    return await query
                        .OrderByDescending(l => l.CreatedAt)
                        .Take(20)
                        .ToListAsync();
            }
        }
        // Обновить данные лота
        public async Task<bool> EditLotAsync(Lot updatedLot, IFormFile? newImage,
            int currentUserId)
        {
            try
            {
                var existingLot = await db.Lots
                    .FirstOrDefaultAsync(l => l.Id == updatedLot.Id &&
                    l.SellerId == currentUserId); 

                if (existingLot == null) return false;

                // Обновляем только разрешённые поля
                existingLot.Title = updatedLot.Title?.Trim();
                existingLot.DescriptionLot = updatedLot.DescriptionLot?.Trim();               
                existingLot.CurrentPrice = updatedLot.StartPrice;
                existingLot.CategoryId = updatedLot.CategoryId;
                existingLot.EndDate = updatedLot.EndDate;

                // Обработка нового изображения
                if (newImage != null && newImage.Length > 0)
                {
                    // Удаляем старое изображение
                    if (!string.IsNullOrEmpty(existingLot.ImageUrl))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(),
                            "wwwroot", existingLot.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    // Генерируем уникальное имя и сохраняем новое
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(newImage.FileName);
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await newImage.CopyToAsync(stream);
                    }
                    existingLot.ImageUrl = $"/images/{fileName}";
                }

                // Если лот был на проверке и его редактируют — возвращаем на модерацию
                existingLot.Status = LotStatus.Review;

                db.Lots.Update(existingLot);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления лота: {ex.Message}");
                return false;
            }
        }



    }
}
