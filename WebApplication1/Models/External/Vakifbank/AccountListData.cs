using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class AccountListData
    {
        [JsonPropertyName("accounts")]
        public List<VakifbankAccount> Accounts { get; set; } = new List<VakifbankAccount>();
    }

    public class AccountListResponse
    {
        [JsonPropertyName("data")]
        public AccountListData Data { get; set; } = new AccountListData();
    }

    public class VakifbankAccount
    {
        [JsonPropertyName("CurrencyCode")]
        public string CurrencyCode { get; set; } = string.Empty;

        [JsonPropertyName("LastTransactionDate")]
        public DateTime LastTransactionDate { get; set; }

        [JsonPropertyName("AccountStatus")]
        public string AccountStatus { get; set; } = string.Empty;

        [JsonPropertyName("IBAN")]
        public string Iban { get; set; } = string.Empty;

        [JsonPropertyName("RemainingBalance")]
        public decimal RemainingBalance { get; set; }

        [JsonPropertyName("Balance")]
        public decimal Balance { get; set; }

        [JsonPropertyName("AccountType")]
        public int AccountType { get; set; }

        [JsonPropertyName("AccountNumber")]
        public String AccountNumber { get; set; } = string.Empty;


    }
}
