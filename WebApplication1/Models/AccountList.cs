using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class AccountList
    {
        [Key]
        public int Id { get; set; }
        public String AccountNumber { get; set; } = string.Empty; //Hesap numarası
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } //Hesap bakiyesi
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingBalance { get; set; } //Kullanılabilir bakiye
        public string IBAN { get; set;} = string.Empty; //Uluslararası banka hesap numarası
        public string CurrencyCode { get; set; } = string.Empty; //Döviz kodu
        public string AccountStatus { get; set; } = string.Empty; //Hesap durumu, A: Açık, K: Kapalı
        public DateTime LastTransactionDate { get; set; } //Son işlem tarihi
        public int AccountType { get; set; } //Hesap tipi
        public int UserId { get; set; }
        public string ProviderName { get; set; } = "Internal";

        [ForeignKey("UserId")]
        public User? User { get; set; }



        public DateTime? OpeningDate { get; set; }
        public string? CustomerNumber { get; set; }
        public string? BranchCode { get; set; }

        public List<AccountTransaction> Transactions { get; set; } = new List<AccountTransaction>();
    }
}
