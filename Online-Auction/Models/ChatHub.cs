using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Services;
using System.Security.Claims;

namespace Online_Auction.Models
{
    public class ChatHub : Hub
    {
        private readonly AuctionDbContext db;
        public ChatHub(AuctionDbContext db)
        {
            this.db = db;
        }

        // Отправка сообщения в чат
        public async Task Send(string message)
        {            
            await Clients.All.SendAsync("Receive", message);
        }  

        // Рассылка обновления цены
        public async Task SendNewPrice(int lotId, decimal newPrice, string userName)
        {
            await Clients.Group($"lot-{lotId}").SendAsync("PriceUpdated", new
            {
                LotId = lotId,
                newPrice = newPrice,
                UserName = userName,
                Time = DateTime.Now
            });
        }

        // Вход в группу лота
        public async Task JoinLotGroup(int lotId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"lot-{lotId}");
        }
    }
}
