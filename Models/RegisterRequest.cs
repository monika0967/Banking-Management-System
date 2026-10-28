
using System.ComponentModel.DataAnnotations;

namespace BankingProject.Models
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Customer|Admin)$")]
        public string UserRole { get; set; } = "Customer";
    }
}

