namespace WebApplication1.Models.DTOs
{
    public class TransactionDTO
    {
        public string TransactionId { get; set; } = string.Empty;
        public string TransactionName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
