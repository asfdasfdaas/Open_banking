using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class AccountListResponse
    {
        [JsonPropertyName("data")]
        public AccountListData Data { get; set; } = new AccountListData();
    }
}
