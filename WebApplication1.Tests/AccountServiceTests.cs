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

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoAccountsExist()
        {
            // Arrange
            int testUserId = 100;
            var mockRepo = new Mock<IAccountRepository>();

            mockRepo.Setup(repo => repo.GetUserAccountsAsync(testUserId))
                    .ReturnsAsync(new List<AccountList>()); // Return an empty list

            var accountService = new AccountService(mockRepo.Object);
            // Act
            var result = await accountService.GetAllAsync(testUserId);
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result is an empty collection

            mockRepo.Verify(repo => repo.GetUserAccountsAsync(testUserId), Times.Once);
        }


        [Fact]
        public async Task GetBtIdAsync_ShouldReturnMappedDto_WhenAccountExists()
        {
            // Arrange
            int testUserId = 99;
            int testAccountId = 1;
            var mockRepo = new Mock<IAccountRepository>();
            var fakeAccount = new AccountList { Id = testAccountId, UserId = testUserId, AccountNumber = "123456789", Balance = 1500m, CurrencyCode = "TRY" };

            mockRepo.Setup(repo => repo.GetByIdAsync(testAccountId, testUserId))
                    .ReturnsAsync(fakeAccount);

            var accountService = new AccountService(mockRepo.Object);

            // Act
            var result = await accountService.GetByIdAsync(testAccountId, testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeAccount.AccountNumber, result.AccountNumber);
            Assert.Equal(fakeAccount.Balance, result.Balance);
            Assert.Equal(fakeAccount.CurrencyCode, result.CurrencyCode);

            mockRepo.Verify(repo => repo.GetByIdAsync(testAccountId, testUserId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenAccountDoesNotExist()
        {
            // Arrange
            int testUserId = 99;
            int testAccountId = 999; // Assume this ID does not exist
            var mockRepo = new Mock<IAccountRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(testAccountId, testUserId))
                    .ReturnsAsync((AccountList)null); // Return null to simulate not found
            var accountService = new AccountService(mockRepo.Object);
            // Act
            var result = await accountService.GetByIdAsync(testAccountId, testUserId);
            // Assert
            Assert.Null(result); // Expecting null when account is not found
            mockRepo.Verify(repo => repo.GetByIdAsync(testAccountId, testUserId), Times.Once);
        }
    }
}