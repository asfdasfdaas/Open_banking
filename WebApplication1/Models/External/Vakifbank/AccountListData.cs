using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class AccountListData
    {
        [JsonPropertyName("accounts")]
        public List<VakifbankAccount> Accounts { get; set; } = new List<VakifbankAccount>();
    }
}
