using WebApplication1.Models;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Mapper
{
    public static class AccountMapper
    {
        public static AccountListDTO ToAccountDto(this AccountList accountModel)
        {
            return new AccountListDTO
            {
                AccountNumber = accountModel.AccountNumber,
                Balance = accountModel.Balance,
                RemainingBalance = accountModel.RemainingBalance,
                IBAN = accountModel.IBAN,
                CurrencyCode = accountModel.CurrencyCode,
                AccountStatus = accountModel.AccountStatus,
                LastTransactionDate = accountModel.LastTransactionDate,
                AccountType = accountModel.AccountType
            };
        }
    }
}
