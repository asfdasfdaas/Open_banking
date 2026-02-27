using WebApplication1.Models;
using WebApplication1.Models.DTOs;
using System;

namespace WebApplication1.Mapper
{
    public static class AccountCreateMapper
    {
    public static AccountList ToAccountFromCreateDTO(this AccountCreateDTO createDto)
    {
        return new AccountList
        {
            AccountNumber = createDto.AccountNumber,
            IBAN = createDto.IBAN,
            CurrencyCode = createDto.CurrencyCode,
            AccountType = createDto.AccountType,

            Balance = 0,
            RemainingBalance = 0,
            AccountStatus = "A",
            LastTransactionDate = DateTime.Now
        };
    }
}
}
