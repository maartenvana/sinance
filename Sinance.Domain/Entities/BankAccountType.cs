using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Sinance.Domain.Entities
{
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Database enum")]
    public enum BankAccountType
    {
        [Display(Name = "Betaalrekening")]
        Checking = 1,

        [Display(Name = "Spaarrekening")]
        Savings = 2,

        [Display(Name = "Beleggingsrekening")]
        Investment = 3,

        [Display(Name = "Kind Betaalrekening")]
        ChildChecking = 4,

        [Display(Name = "Kind Spaarrekening")]
        ChildSavings = 5
    }
}