
namespace Sinance.Communication.Model.Shared
{
    public class AmountAndPercentage
    {
        public AmountAndPercentage(decimal amount, decimal percentage)
        {
            Amount = amount;
            Percentage = percentage;
        }

        public decimal Amount { get; set; }

        public decimal Percentage { get; set; }
    }
}
