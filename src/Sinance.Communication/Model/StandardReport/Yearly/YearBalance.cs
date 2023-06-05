namespace Sinance.Communication.Model.StandardReport.Yearly;

public class YearBalance
{
    public decimal Difference => End - Start;
    public decimal End { get; private set; }
    public decimal Start { get; private set; }

    public YearBalance(decimal start, decimal end)
    {
        Start = start;
        End = end;
    }
}