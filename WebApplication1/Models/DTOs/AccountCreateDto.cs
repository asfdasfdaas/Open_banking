using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.DTOs
{
    public class AccountCreateDTO
    {
        [Required]
        public String AccountNumber { get; set; } = string.Empty; //Hesap numarası

        [Range(0, double.MaxValue, ErrorMessage = "Initial balance cannot be negative.")]
        public decimal Balance { get; set; } //Hesap bakiyesi

        [Range(0, double.MaxValue)]
        public decimal RemainingBalance { get; set; } //Kullanılabilir bakiye

        [Required]
        [StringLength(26, MinimumLength = 26, ErrorMessage = "IBAN must be exactly 26 characters.")]
        public string IBAN { get; set; } = string.Empty; //Uluslararası banka hesap numarası

        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 letters (e.g., TRY, USD).")]
        public string CurrencyCode { get; set; } = string.Empty; //Döviz kodu

        [Required]
        public string AccountStatus { get; set; } = string.Empty; //Hesap durumu, A: Açık, K: Kapalı
        public DateTime LastTransactionDate { get; set; }//Son işlem tarihi

        [Required]
        public int AccountType { get; set; } //Hesap tipi
    }
}
