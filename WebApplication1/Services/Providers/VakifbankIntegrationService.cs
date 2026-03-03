using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication1.Models.DTOs;
using WebApplication1.Models.External.Vakifbank;
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
        }

        public async Task<string> GetBankTokenAsync()
        {
            var requestBody = new Dictionary<string, string>
                {
                    { "client_id", _config["Vakifbank:ClientId"]! }, // Store these in User Secrets!
                    { "client_secret", _config["Vakifbank:ClientSecret"]! },
                    { "grant_type", "b2b_credentials" },
                    { "scope", "account" },
                    { "consentId", _config["Vakifbank:ConsentId"]! }, // Usually dynamic, but static for testing
                    { "resource", "sandbox" }
                };

            var content = new FormUrlEncodedContent(requestBody);

            var response = await _httpClient.PostAsync("/oauth2/token", content); // Replace with actual token URL path
            response.EnsureSuccessStatusCode(); // Crashes safely if you get a 400 or 401

            var jsonString = await response.Content.ReadAsStringAsync();

            var tokenData = JsonSerializer.Deserialize<TokenResponse>(jsonString);

            return tokenData!.AccessToken;
        }

        public async Task<IEnumerable<AccountListDTO>> GetAccountsFromBankAsync(int userId)
        {
            var token = await GetBankTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var emptybody = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/accountList", emptybody);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };

            
            var result = JsonSerializer.Deserialize<AccountListResponse>(jsonString, options);

            var accounts = result?.Data?.Accounts;
            if (accounts == null || accounts.Count == 0)
                return Enumerable.Empty<AccountListDTO>();

            return accounts.Select(a => new AccountListDTO
            {
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                RemainingBalance = a.RemainingBalance,
                IBAN = a.Iban, // map VakifbankAccount.Iban -> DTO.IBAN
                CurrencyCode = a.CurrencyCode,
                AccountStatus = a.AccountStatus,
                LastTransactionDate = a.LastTransactionDate,
                AccountType = a.AccountType
            });
        }
    }
}
