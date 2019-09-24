using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Sinance.Domain.Entities
{
    /// <summary>
    /// Column type entity
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Conversion from the database, no 0 possible")]
    public enum ColumnType
    {
        /// <summary>
        /// Ignore type
        /// </summary>
        [Display(Name = "Negeren")]
        Ignore = 1,

        /// <summary>
        /// Date type
        /// </summary>
        [Display(Name = "Datum")]
        Date = 2,

        /// <summary>
        /// Add / substract type
        /// </summary>
        [Display(Name = "Af/bij")]
        AddSubtract = 3,

        /// <summary>
        /// Amount type
        /// </summary>
        [Display(Name = "Hoeveelheid")]
        Amount = 4,

        /// <summary>
        /// Description type
        /// </summary>
        [Display(Name = "Omschrijving")]
        Description = 5,

        /// <summary>
        /// Name type
        /// </summary>
        [Display(Name = "Naam")]
        Name = 6,

        /// <summary>
        /// Destination account type
        /// </summary>
        [Display(Name = "Tegenrekening")]
        DestinationAccount = 7,

        /// <summary>
        /// Bank account from type
        /// </summary>
        [Display(Name = "Rekening")]
        BankAccountFrom = 8
    }
}