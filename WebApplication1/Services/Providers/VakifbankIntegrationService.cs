using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
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
        private readonly IMemoryCache _cache;

        public VakifbankIntegrationService(HttpClient httpClient, IConfiguration config, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _config = config;
            _cache = cache;
        }

        public async Task<string> GetBankTokenAsync(string consentId)
        {
            string cacheKey = $"VakifbankToken_{consentId}";

            if (_cache.TryGetValue(cacheKey, out string? cachedToken))
            {
                return cachedToken!; // Cache hit! Return it immediately.
            }

            var requestBody = new Dictionary<string, string>
            {
                { "client_id", _config["Vakifbank:ClientId"]! },
                { "client_secret", _config["Vakifbank:ClientSecret"]! },
                { "grant_type", "b2b_credentials" },
                { "scope", "account" },
                { "consentId", consentId },
                { "resource", "sandbox" }
            };

            var content = new FormUrlEncodedContent(requestBody);

            var response = await _httpClient.PostAsync("/oauth2/token", content); // Replace with actual token URL path
            response.EnsureSuccessStatusCode(); // Crashes if you get a 400 or 401

            var jsonString = await response.Content.ReadAsStringAsync();

            var tokenData = JsonSerializer.Deserialize<TokenResponse>(jsonString);

            var newToken = tokenData!.AccessToken;

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(50));

            _cache.Set(cacheKey, newToken, cacheOptions);

            return newToken;
        }

        public async Task<String> GetClientCKeyAsync()
        {
            string cacheKey = "VakifbankCCToken";

            if (_cache.TryGetValue(cacheKey, out string? cachedToken))
            {
                return cachedToken!; // Cache hit! Return it immediately.
            }

            var requestBody = new Dictionary<string, string>
            {
                { "client_id", _config["Vakifbank:SecondClientId"]! },
                { "client_secret", _config["Vakifbank:SecondClientKey"]! },
                { "grant_type", "client_credentials" },
                { "scope", "public oob" }
            };

            var content = new FormUrlEncodedContent(requestBody);

            var response = await _httpClient.PostAsync("/oauth2/token", content);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            var tokenData = JsonSerializer.Deserialize<TokenResponse>(jsonString);

            var newToken = tokenData!.AccessToken;

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(50)); // Safely caches for 50 mins

            _cache.Set(cacheKey, newToken, cacheOptions);

            return newToken;
        }

        public async Task<IEnumerable<AccountListDTO>> GetAccountsFromBankAsync(int userId, string consentId)
        {
            var token = await GetBankTokenAsync(consentId);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var emptybody = new StringContent("{}", Encoding.UTF8, "application/json");

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

        public async Task<AccountDetailDTO> GetAccountDetailAsync(string accountNumber, string consentId)
        {
            var token = await GetBankTokenAsync(consentId);
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
                BranchCode = info.BranchCode,
                ProviderName = info.ProviderName
            };
        }

        public async Task<IEnumerable<TransactionDTO>> GetAccountTransactionsAsync(string accountNumber, DateTime startDate, DateTime endDate, string consentId)
        {
            var token = await GetBankTokenAsync(consentId);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Build the request body EXACTLY like Postman
            var requestBody = JsonSerializer.Serialize(new
            {
                AccountNumber = accountNumber,
                StartDate = startDate.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                EndDate = endDate.ToString("yyyy-MM-ddTHH:mm:sszzz")
            });

            var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/accountTransactions", content); // Update URL if needed

            if (!response.IsSuccessStatusCode)
            {
                //  Read the actual error message the bank sent back
                var errorBody = await response.Content.ReadAsStringAsync();

                //  Throw a custom exception that includes the bank's detailed complaint!
                throw new Exception($"Bank API rejected the request. Status: {response.StatusCode}. Bank Details: {errorBody}");
            }

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
        public async Task<byte[]> GetReceiptPdfAsync(string transactionId, string accountNumber, string consentId)
        {
            var token = await GetBankTokenAsync(consentId);
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

            if (!response.IsSuccessStatusCode)
            {
                // 2. Read the actual error message the bank sent back
                var errorBody = await response.Content.ReadAsStringAsync();

                // 3. Throw a custom exception that includes the bank's detailed complaint!
                throw new Exception($"Bank API rejected the request. Status: {response.StatusCode}. Bank Details: {errorBody}");
            }

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

        public async Task<decimal> CalculateCurrencyAsync(string sourceCurrency, decimal amount, string targetCurrency)
        {
            // 1. Get the token (from cache, or fetch a new one)
            var token = await GetClientCKeyAsync();

            // 2. Build the exact JSON payload the bank expects
            var payload = new
            {
                SourceCurrencyCode = sourceCurrency,
                SourceAmount = amount.ToString("0.##"), // Ensures it's formatted cleanly (e.g. "100" or "100.50")
                TargetCurrencyCode = targetCurrency
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // 3. Create the request and attach the Bearer token
            var request = new HttpRequestMessage(HttpMethod.Post, "/currencyCalculator")
            {
                Content = jsonContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 4. Send it!
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Currency API failed: {errorBody}");
            }

            // 5. Parse the result and return the final SaleAmount
            var responseJson = await response.Content.ReadAsStringAsync();

            // We use JsonSerializerOptions to ignore case, since C# properties are PascalCase but JSON might vary
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resultData = JsonSerializer.Deserialize<CurrencyCalculatorResponse>(responseJson, options);

            // Convert the string "2.24" into a C# decimal
            if (decimal.TryParse(resultData.Data.Currency.SaleAmount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal finalAmount))
            {
                return finalAmount;
            }

            throw new Exception("Failed to parse the sale amount from the bank.");
        }

        public async Task<DepositProductResponse> GetDepositProductsAsync()
        {
            // 1. Get the shared token
            var token = await GetClientCKeyAsync();

            // 2. Prepare an empty JSON body "{}" as required
            var jsonContent = new StringContent("{}", Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "/depositProductList")
            {
                Content = jsonContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 3. Send the request
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Deposit Product API failed: {errorBody}");
            }

            // 4. Parse and return the data
            var responseJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var resultData = JsonSerializer.Deserialize<DepositProductResponse>(responseJson, options);
            return resultData!;
        }

        public async Task<BranchListResponse> GetBranchListAsync(string? cityCode = null, string? districtCode = null)
        {
            // 1. Get the shared token
            var token = await GetClientCKeyAsync();

            // 2. Build the dynamic payload
            var payload = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(cityCode))
            {
                payload.Add("CityCode", cityCode);
            }
            if (!string.IsNullOrWhiteSpace(districtCode))
            {
                payload.Add("BankDistrictCode", districtCode);
            }

            var jsonString = JsonSerializer.Serialize(payload);
            var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // 3. Create and send the request
            var request = new HttpRequestMessage(HttpMethod.Post, "/vakifbankBranchList")
            {
                Content = jsonContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Branch List API failed: {errorBody}");
            }

            // 4. Parse the result
            var responseJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var resultData = JsonSerializer.Deserialize<BranchListResponse>(responseJson, options);
            return resultData!;
        }

        public async Task<DepositCalculatorResponse> CalculateDepositAsync(DepositCalculatorRequest request)
        {
            // 1. Get the shared token
            var token = await GetClientCKeyAsync();

            // 2. Turn our C# request object into JSON
            var jsonString = JsonSerializer.Serialize(request);
            var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // 3. Create and send the HTTP POST request
            var newRequest = new HttpRequestMessage(HttpMethod.Post, "/depositCalculator")
            {
                Content = jsonContent
            };
            newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(newRequest);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Deposit Calculator API failed: {errorBody}");
            }

            // 4. Parse the result into our Response DTO
            var responseJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var resultData = JsonSerializer.Deserialize<DepositCalculatorResponse>(responseJson, options);
            return resultData!;
        }

        public async Task<ATMListResponse> GetATMListAsync(string? cityCode = null, string? districtCode = null)
        {
            // 1. Get the shared token
            var token = await GetClientCKeyAsync();

            // 2. Build the dynamic payload
            var payload = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(cityCode))
            {
                payload.Add("CityCode", cityCode);
            }
            if (!string.IsNullOrWhiteSpace(districtCode))
            {
                payload.Add("DistrictCode", districtCode); // Notice: ATMs use DistrictCode, not BankDistrictCode
            }

            var jsonString = JsonSerializer.Serialize(payload);
            var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // 3. Create and send the request
            var request = new HttpRequestMessage(HttpMethod.Post, "/vakifbankATMList")
            {
                Content = jsonContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"ATM List API failed: {errorBody}");
            }

            // 4. Parse the result safely
            var responseJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var resultData = JsonSerializer.Deserialize<ATMListResponse>(responseJson, options);
            return resultData!;
        }

        public async Task<CityListResponse> GetCityListAsync()
        {
            // 1. Get the shared token
            var token = await GetClientCKeyAsync();

            // 2. Prepare an empty JSON body "{}" as required
            var jsonContent = new StringContent("{}", Encoding.UTF8, "application/json");

            // 3. Create and send the request
            var request = new HttpRequestMessage(HttpMethod.Post, "/cityList")
            {
                Content = jsonContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"City List API failed: {errorBody}");
            }

            // 4. Parse the result safely
            var responseJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var resultData = JsonSerializer.Deserialize<CityListResponse>(responseJson, options);
            return resultData!;
        }

        public async Task<DistrictListResponse> GetDistrictListAsync(string cityCode)
        {
            // 1. Get the shared token
            var token = await GetClientCKeyAsync();

            // 2. Build the JSON payload with the required CityCode
            var payload = new { CityCode = cityCode };
            var jsonString = JsonSerializer.Serialize(payload);
            var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // 3. Create and send the request
            var request = new HttpRequestMessage(HttpMethod.Post, "/districtList")
            {
                Content = jsonContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"District List API failed: {errorBody}");
            }

            // 4. Parse the result safely
            var responseJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var resultData = JsonSerializer.Deserialize<DistrictListResponse>(responseJson, options);
            return resultData!;
        }

        public async Task<NeighborhoodListResponse> GetNeighborhoodListAsync(string districtCode)
        {
            // 1. Get the shared token
            var token = await GetClientCKeyAsync();

            // 2. Build the JSON payload with the required DistrictCode
            var payload = new { DistrictCode = districtCode };
            var jsonString = JsonSerializer.Serialize(payload);
            var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // 3. Create and send the request
            var request = new HttpRequestMessage(HttpMethod.Post, "/neighborhoodList")
            {
                Content = jsonContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Neighborhood List API failed: {errorBody}");
            }

            // 4. Parse the result safely
            var responseJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var resultData = JsonSerializer.Deserialize<NeighborhoodListResponse>(responseJson, options);
            return resultData!;
        }
    }
}
