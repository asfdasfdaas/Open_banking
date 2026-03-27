namespace WebApplication1.Models.DTOs
{
    public class AccountUpdateDTO
    {
        public string CurrencyCode { get; set; } = string.Empty; //Döviz kodu
        public string AccountStatus { get; set; } = string.Empty; //Hesap durumu, A: Açık, K: Kapalı
        public int AccountType { get; set; } = 1; //Hesap tipi
    }
}
