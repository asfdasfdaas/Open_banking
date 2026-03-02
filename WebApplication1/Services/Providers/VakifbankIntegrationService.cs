using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Services.Providers
{
    public class VakifbankIntegrationService : IBankIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public VakifbankIntegrationService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;

            _httpClient.BaseAddress = new Uri(_config["Vakifbank:BaseUrl"]!);
        }

        public async Task<string> GetBankTokenAsync()
        {
            return "Will be implemented in the future";
        }

        public async Task<IEnumerable<AccountListDTO>> GetAccountsFromBankAsync(int userId)
        {
            return new List<AccountListDTO>();
        }
    }
}
