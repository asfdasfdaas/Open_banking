using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class TransactionResponse
    {
        [JsonPropertyName("Data")]
        public TransactionData Data { get; set; } = new TransactionData();
    }

    public class TransactionData
    {
        [JsonPropertyName("AccountTransactions")]
        [JsonConverter(typeof(SingleOrArrayConverter<VakifbankTransaction>))]
        public List<VakifbankTransaction> AccountTransactions { get; set; } = new List<VakifbankTransaction>();
    }

    public class VakifbankTransaction
    {
        [JsonPropertyName("TransactionId")]
        public string TransactionId { get; set; } = string.Empty;

        [JsonPropertyName("TransactionName")]
        public string TransactionName { get; set; } = string.Empty;

        [JsonPropertyName("TransactionCode")]
        public string TransactionCode { get; set; } = string.Empty;

        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("TransactionType")]
        public string TransactionType { get; set; } = string.Empty;

        [JsonPropertyName("CurrencyCode")]
        public string CurrencyCode { get; set; } = string.Empty;

        [JsonPropertyName("Amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("Balance")]
        public decimal Balance { get; set; }

        [JsonPropertyName("TransactionDate")]
        public DateTime TransactionDate { get; set; }
    }
}
