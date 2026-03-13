namespace WebApplication1.Models.DTOs
{
    public class AccountDetailDTO
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal RemainingBalance { get; set; }
        public string IBAN { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        public DateTime LastTransactionDate { get; set; }
        public int AccountType { get; set; }

        public DateTime? OpeningDate { get; set; }
        public string CustomerNumber { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
    }
}