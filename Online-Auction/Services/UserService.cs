using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class UserService
    {
        private readonly AuctionDbContext db;
        private readonly PasswordHasher<User> passwordHasher;

        // Константа попыток
        private const int MaxFailedAttempts = 3;
        public UserService(AuctionDbContext context)
        {
            db = context;
            passwordHasher = new PasswordHasher<User>();
        }
        public async Task<bool> AddUserAsync(User user, string plainPassword)
        {
            if (await db.Users.AnyAsync(u => u.LoginUs == user.LoginUs ||
            u.Email == user.Email))
                return false;

            // Хешируем пароль
            user.PasswordHash = passwordHasher.HashPassword
                (user, plainPassword);
            var userRole = await db.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "User");
            if (userRole != null)
                user.RoleId = userRole.Id;

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            return true;
        }
        public async Task<List<User>> GetUsersAllAsync()
        {
            return await db.Users
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
        }
        public async Task<User?> FindByLoginAsync(string login,
            string password)
        {
            var user = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.LoginUs == login);

            if (user is null) return null;

            // Проверяем пароль через хеш
            var result = passwordHasher
                .VerifyHashedPassword(user, user.PasswordHash, password);

            return result == PasswordVerificationResult
                .Success ? user : null;
        }
        public async Task<bool> IsLoginEmailTakenAsync(string login, string email)
        {
            return await db.Users.AnyAsync(u => u.LoginUs == login ||
            u.Email == email);
        }
        public async Task<bool> ToggleBanStatusAsync(int userId, bool ban)
        {
            try
            {
                var user = await db.Users.FindAsync(userId);
                if (user == null) return false;

                user.IsBanned = ban;

                db.Users.Update(user);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return false;
            }
        }
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await db.Users.FindAsync(userId);
        }

        // Записать неудачную попытку и вернуть, нужно ли блокировать
        public async Task<bool> RecordFailedLoginAsync(string login)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.LoginUs == login);
            if (user == null) return false; // Пользователь не найден — не считаем

            user.FailedLoginAttempts++;

            // Если достигнут лимит — блокируем
            if (user.FailedLoginAttempts >= MaxFailedAttempts)         
                user.IsBanned = true;
         

            db.Users.Update(user);
            await db.SaveChangesAsync();

            return user.IsBanned == true;
        }

        // Сбросить счётчик при успешном входе
        public async Task ResetFailedLoginAsync(User user)
        {
            if (user.FailedLoginAttempts > 0)
            {
                user.FailedLoginAttempts = 0;
                db.Users.Update(user);
                await db.SaveChangesAsync();
            }
        }

        // Разблокировать пользователя (для админа)
        public async Task<bool> UnlockUserAsync(int userId)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) return false;

            user.FailedLoginAttempts = 0;
            user.IsBanned = false;

            db.Users.Update(user);
            await db.SaveChangesAsync();

            return true;
        }

        // Найти пользователя по логину ИЛИ email
        public async Task<User?> FindByContactAsync(string contact)
        {
            if (string.IsNullOrWhiteSpace(contact))
                return null;

            return await db.Users
                .FirstOrDefaultAsync(u =>
                    u.LoginUs == contact || u.Email == contact);
        }

    }
}
