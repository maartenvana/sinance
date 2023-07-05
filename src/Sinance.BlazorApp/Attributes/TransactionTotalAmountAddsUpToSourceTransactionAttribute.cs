using Sinance.BlazorApp.Business.Model.Transaction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Sinance.BlazorApp.Attributes;

/// <summary>
/// Provides conditional validation based on related property value.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class TransactionTotalAmountAddsUpToSourceTransactionAttribute : ValidationAttribute
{
    #region Properties

    /// <summary>
    /// Gets or sets the other property name that will be used during validation.
    /// </summary>
    /// <value>
    /// The other property name.
    /// </value>
    public string OtherProperty { get; private set; }

    /// <summary>
    /// Gets or sets the display name of the other property.
    /// </summary>
    /// <value>
    /// The display name of the other property.
    /// </value>
    public string OtherPropertyDisplayName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether other property's value should match or differ from provided other property's value (default is <c>false</c>).
    /// </summary>
    /// <value>
    ///   <c>true</c> if other property's value validation should be inverted; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// How this works
    /// - true: validated property is required when other property doesn't equal provided value
    /// - false: validated property is required when other property matches provided value
    /// </remarks>
    public bool IsInverted { get; set; }

    /// <summary>
    /// Gets a value that indicates whether the attribute requires validation context.
    /// </summary>
    /// <returns><c>true</c> if the attribute requires validation context; otherwise, <c>false</c>.</returns>
    public override bool RequiresValidationContext
    {
        get { return true; }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionTotalAmountAddsUpToSourceTransactionAttribute"/> class.
    /// </summary>
    /// <param name="otherProperty">The other property.</param>
    /// <param name="otherPropertyValue">The other property value.</param>
    public TransactionTotalAmountAddsUpToSourceTransactionAttribute(string otherProperty)
        : base("Total amount of transactions should add up to source transaction")
    {
        OtherProperty = otherProperty;
        IsInverted = false;
    }

    #endregion

    /// <summary>
    /// Applies formatting to an error message, based on the data field where the error occurred.
    /// </summary>
    /// <param name="name">The name to include in the formatted message.</param>
    /// <returns>
    /// An instance of the formatted error message.
    /// </returns>
    public override string FormatErrorMessage(string name)
    {
        return base.ErrorMessageString;
    }

    /// <summary>
    /// Validates the specified value with respect to the current validation attribute.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>
    /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
    /// </returns>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (validationContext == null)
        {
            throw new ArgumentNullException(nameof(validationContext));
        }

        PropertyInfo otherProperty = validationContext.ObjectType.GetProperty(OtherProperty);
        if (otherProperty == null)
        {
            return new ValidationResult(
                string.Format(CultureInfo.CurrentCulture, "Could not find a property named '{0}'.", OtherProperty));
        }

        object otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

        if(otherValue is TransactionModel transactionModel &&
            value is IEnumerable<TransactionModel> transactions)
        {
            if(transactions.Sum(x => x.Amount) > transactionModel.Amount)
            {
                return new ValidationResult("New transactions total amount is larger than source transaction");
            }
            else if(transactions.Sum(x => x.Amount) < transactionModel.Amount)
            {
                return new ValidationResult("New transactions total amount is lower than source transaction");
            }
        } 
        else
        {
            return new ValidationResult("Incorrect attribute usage"); // TODO: Better message
        }

        return ValidationResult.Success;
    }
}
