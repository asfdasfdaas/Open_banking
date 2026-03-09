using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class User
    {
        [Key]
        public int Id { set; get; }
        [Required]
        public string UserName { set; get; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? VakifbankConsentId { get; set; }

        //Navigation property to link User with their Bank Accounts
        public List<AccountList> BankAccounts { get; set; } = new List<AccountList>();
    }
}
