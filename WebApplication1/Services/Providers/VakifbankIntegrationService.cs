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
                { "client_id", _config["Vakifbank:ClientId"]! },
                { "client_secret", _config["Vakifbank:ClientSecret"]! },
                { "grant_type", "b2b_credentials" },
                { "scope", "account" },
                { "consentId", _config["Vakifbank:ConsentId"]! },
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

            // 1. Tell C# to allow Strings to be converted to Decimals automatically
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };

            // 2. Unpack the JSON using the options
            var result = JsonSerializer.Deserialize<AccountListResponse>(jsonString, options);

            var accounts = result?.Data?.Accounts;
            if (accounts == null || accounts.Count == 0)
                return Enumerable.Empty<AccountListDTO>();

            return accounts.Select(a => new AccountListDTO
            {
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                RemainingBalance = a.RemainingBalance,
                IBAN = a.Iban,
                CurrencyCode = a.CurrencyCode,
                AccountStatus = a.AccountStatus,
                LastTransactionDate = a.LastTransactionDate,
                AccountType = a.AccountType
            });
        }

        public async Task<AccountDetailDTO> GetAccountDetailAsync(string accountNumber)
        {
            var token = await GetBankTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestBody = JsonSerializer.Serialize(new { AccountNumber = accountNumber });
            var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/accountDetail", content); // Adjust URL path to match Postman
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };

            var result = JsonSerializer.Deserialize<AccountDetailResponse>(jsonString, options);

            var info = result?.Data?.AccountInfo;
            if (info == null) return null;

            return new AccountDetailDTO
            {
                AccountNumber = info.AccountNumber,
                Balance = info.Balance,
                RemainingBalance = info.RemainingBalance,
                IBAN = info.Iban,
                CurrencyCode = info.CurrencyCode,
                AccountStatus = info.AccountStatus,
                LastTransactionDate = info.LastTransactionDate,
                AccountType = info.AccountType,
                OpeningDate = info.OpeningDate,
                CustomerNumber = info.CustomerNumber,
                BranchCode = info.BranchCode
            };
        }

        public async Task<IEnumerable<TransactionDTO>> GetAccountTransactionsAsync(string accountNumber, DateTime startDate, DateTime endDate)
        {
            var token = await GetBankTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Build the request body EXACTLY like your Postman example
            var requestBody = JsonSerializer.Serialize(new
            {
                AccountNumber = accountNumber,
                StartDate = startDate.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                EndDate = endDate.ToString("yyyy-MM-ddTHH:mm:sszzz")
            });

            var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/accountTransactions", content); // Update URL if needed
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };

            var result = JsonSerializer.Deserialize<TransactionResponse>(jsonString, options);

            var transactions = result?.Data?.AccountTransactions;
            if (transactions == null) return Enumerable.Empty<TransactionDTO>();

            return transactions.Select(t => new TransactionDTO
            {
                TransactionId = t.TransactionId,
                TransactionName = t.TransactionName,
                Description = t.Description,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                Balance = t.Balance,
                TransactionDate = t.TransactionDate
            });
        }
        public async Task<byte[]> GetReceiptPdfAsync(string transactionId, string accountNumber)
        {
            var token = await GetBankTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1. Build the JSON body
            var requestBody = JsonSerializer.Serialize(new
            {
                TransactionId = transactionId,
                AccountNumber = accountNumber,
                ReceiptFormat = "1"
            });

            var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            // 2. Send the request 
            var response = await _httpClient.PostAsync("/getReceipt", content);
            response.EnsureSuccessStatusCode();

            // 3. Unpack the JSON
            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<ReceiptResponse>(jsonString, options);

            var base64String = result?.Documents?.PdfReceipt;

            if (string.IsNullOrEmpty(base64String))
            {
                throw new Exception("The bank did not return a valid PDF string.");
            }

            // 4. Convert the giant string of text back into a raw PDF file
            return Convert.FromBase64String(base64String);
        }
    }
}
