namespace Online_Auction.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        // Контактные данные (логин или email)
        public string SenderContact { get; set; } = null!;

        // Тема письма
        public string Subject { get; set; } = null!;
        public string MessageText { get; set; } = null!;    
        public bool IsRead { get; set; } = false;
    }
}
