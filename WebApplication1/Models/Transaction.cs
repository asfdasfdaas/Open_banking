using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Positive for Credit, Negative for Debit
        public string TransactionCode { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string TransactionName { get; set; }

        [Required]
        public string TransactionType { get; set; } = string.Empty; // 1 to account, 2 from account

        public string Description { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty; // e.g., "USD", "EUR"

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // Navigation Property: This allows EF to link the two tables
        public AccountList? Account { get; set; }
    }
}
