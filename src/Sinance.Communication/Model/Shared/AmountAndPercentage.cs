namespace Sinance.Communication.Model.Shared;

public class AmountAndPercentage
{
    public decimal Amount { get; set; }

    public decimal Percentage { get; set; }

    public AmountAndPercentage(decimal amount, decimal percentage)
    {
        Amount = amount;
        Percentage = percentage;
    }
}