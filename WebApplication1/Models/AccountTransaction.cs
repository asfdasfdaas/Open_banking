using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class AccountTransaction
    {
        [Key]
        public int Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;

        public string TransactionName { get; set; } = string.Empty;
        public string TransactionCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public DateTime TransactionDate { get; set; }

        // Foreign Key linking back to AccountList table
        public int AccountListId { get; set; }
        [ForeignKey("AccountListId")]
        public AccountList? Account { get; set; }
    }
}
