using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class AccountListData
    {
        [JsonPropertyName("Accounts")]
        public List<VakifbankAccount> Accounts { get; set; } = new List<VakifbankAccount>();
    }
}
