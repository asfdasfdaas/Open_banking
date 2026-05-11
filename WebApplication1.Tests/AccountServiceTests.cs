using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Services;       // Where AccountService lives
using WebApplication1.Interface;      // Where IAccountRepository lives
using WebApplication1.Models;         // Where AccountList lives
using WebApplication1.Models.DTOs;    // Where AccountListDTO lives

namespace WebApplication1.Tests
{
    public class AccountServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedDtos_WhenAccountsExist()
        {
            // ==========================================
            // 1. ARRANGE
            // ==========================================
            int testUserId = 99;

            // Create some fake database records
            var fakeDatabaseAccounts = new List<AccountList>
            {
                new AccountList { Id = 1, UserId = testUserId, AccountNumber = "123456789", Balance = 1500m, CurrencyCode = "TRY" },
                new AccountList { Id = 2, UserId = testUserId, AccountNumber = "987654321", Balance = 500m, CurrencyCode = "USD" }
            };

            // Create the fake repository
            var mockRepo = new Mock<IAccountRepository>();

            // Tell the fake repository: "When GetUserAccountsAsync is called with ID 99, return the fake list."
            // We use ReturnsAsync because the repository method returns a Task.
            mockRepo.Setup(repo => repo.GetUserAccountsAsync(testUserId))
                    .ReturnsAsync(fakeDatabaseAccounts);

            // Inject the fake repository into the real service
            var accountService = new AccountService(mockRepo.Object);

            // ==========================================
            // 2. ACT
            // ==========================================

            // Call the exact method you want to test
            var result = await accountService.GetAllAsync(testUserId);

            // ==========================================
            // 3. ASSERT
            // ==========================================

            // 1. Ensure the result is not null
            Assert.NotNull(result);

            // Convert the IEnumerable to a List so we can easily check inside it
            var resultList = result.ToList();

            // 2. Ensure we got exactly 2 DTOs back
            Assert.Equal(2, resultList.Count);

            // 3. Ensure your mapping logic (ToAccountDto) actually copied the data correctly
            Assert.Equal("123456789", resultList[0].AccountNumber);
            Assert.Equal(1500m, resultList[0].Balance);
            Assert.Equal("USD", resultList[1].CurrencyCode);

            // 4. Verify that the service actually called the database exactly one time
            mockRepo.Verify(repo => repo.GetUserAccountsAsync(testUserId), Times.Once);
        }
    }
}