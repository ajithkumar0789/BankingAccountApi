using System.ComponentModel.DataAnnotations;

namespace BankingAccountApi.Models;

public class CreateAccountRequest
{
    [Required]
    [StringLength(100)]
    public string AccountHolderName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string AccountNumber { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Balance { get; set; }

    [Required]
    [RegularExpression("^(Savings|Current)$",
        ErrorMessage = "Account type must be Savings or Current.")]
    public string AccountType { get; set; } = "Savings";
}

public class UpdateAccountRequest
{
    [Required]
    [StringLength(100)]
    public string AccountHolderName { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Savings|Current)$",
        ErrorMessage = "Account type must be Savings or Current.")]
    public string AccountType { get; set; } = "Savings";
}

public class AmountRequest
{
    public decimal Amount { get; set; }
}