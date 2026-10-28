using System.ComponentModel.DataAnnotations;

public class MoneyRequest
{
    [Required]
    [StringLength(20)]
    public string AccountNumber { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Amount { get; set; }
}

public class TransferRequest
{
    [Required]
    [StringLength(20)]
    public string FromAccountNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ToAccountNumber { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Amount { get; set; }
}
