using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class AccountDetailResponse
    {
        [JsonPropertyName("Data")]
        public AccountDetailData Data { get; set; } = new AccountDetailData();
    }

    public class AccountDetailData
    {
        [JsonPropertyName("AccountInfo")]
        public VakifbankAccountInfo AccountInfo { get; set; } = new VakifbankAccountInfo();
    }

    public class VakifbankAccountInfo
    {
        [JsonPropertyName("CurrencyCode")]
        public string CurrencyCode { get; set; } = string.Empty;

        [JsonPropertyName("LastTransactionDate")]
        public DateTime LastTransactionDate { get; set; }

        [JsonPropertyName("AccountStatus")]
        public string AccountStatus { get; set; } = string.Empty;

        [JsonPropertyName("OpeningDate")]
        public DateTime OpeningDate { get; set; }

        [JsonPropertyName("IBAN")]
        public string Iban { get; set; } = string.Empty;

        [JsonPropertyName("CustomerNumber")]
        public string CustomerNumber { get; set; } = string.Empty;

        [JsonPropertyName("RemainingBalance")]
        public decimal RemainingBalance { get; set; }

        [JsonPropertyName("Balance")]
        public decimal Balance { get; set; }

        [JsonPropertyName("AccountType")]
        public int AccountType { get; set; }

        [JsonPropertyName("BranchCode")]
        public string BranchCode { get; set; } = string.Empty;

        [JsonPropertyName("AccountNumber")]
        public string AccountNumber { get; set; } = string.Empty;
    }
}
