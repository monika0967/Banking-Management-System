using System.ComponentModel.DataAnnotations;

namespace BankingProject.Models
{
    public class AccountRequest
    {
        [Range(1, int.MaxValue)]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AccountType { get; set; } = string.Empty;
    }
    public class AccountUpdate
    {
        [Required]
        [StringLength(20)]
        public string AccountType { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Active|Inactive|Closed)$")]
        public string Status { get; set; } = string.Empty;
    }

}

