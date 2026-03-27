namespace WebApplication1.Models.DTOs
{
    public class TransferDTO
    {
        public string SenderAccountNumber { get; set; } = string.Empty;
        public string ReceiverAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
    }
}
