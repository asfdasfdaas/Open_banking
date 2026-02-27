using WebApplication1.Models;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Mapper
{
    public static class AccountUpdateMapper
    {
        public static void UpdateAccountFromDTO(this AccountList accountModel, AccountUpdateDTO updateDto)
        {
            accountModel.CurrencyCode = updateDto.CurrencyCode;
            accountModel.AccountStatus = updateDto.AccountStatus;
            accountModel.AccountType = updateDto.AccountType;
        }

    }
}
