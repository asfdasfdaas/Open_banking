namespace WebApplication1.Models.DTOs
{
    public class AccountListDTO
    {
        public String AccountNumber { get; set; } = string.Empty; //Hesap numarası
        public decimal Balance { get; set; } //Hesap bakiyesi
        public decimal RemainingBalance { get; set; } //Kullanılabilir bakiye
        public string IBAN { get; set; } = string.Empty; //Uluslararası banka hesap numarası
        public string CurrencyCode { get; set; } = string.Empty; //Döviz kodu
        public string AccountStatus { get; set; } = string.Empty; //Hesap durumu, A: Açık, K: Kapalı
        public DateTime LastTransactionDate { get; set; } //Son işlem tarihi
        public int AccountType { get; set; } //Hesap tipi
    }
}
