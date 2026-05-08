using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.DTOs
{
    public class AccountCreateDTO
    {
        [Range(0, double.MaxValue)]
        public decimal Balance { get; set; } // Initial Deposit

        [Required]
        public string CurrencyCode { get; set; } = string.Empty; // Currency
    }
}
