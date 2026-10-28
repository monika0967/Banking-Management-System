
using System;
using System.ComponentModel.DataAnnotations;

namespace BankingProject.Models
{
    public class CustomerRequest
    {
        public int? UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string FullName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(40)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AadhaarNumber { get; set; } = string.Empty;
    }
}

