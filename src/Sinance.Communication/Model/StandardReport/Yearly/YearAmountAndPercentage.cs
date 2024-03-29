﻿using Sinance.Communication.Model.Shared;

namespace Sinance.Communication.Model.StandardReport.Yearly;

public class YearAmountAndPercentage
{
    public AmountAndPercentage Difference { get; private set; }

    public AmountAndPercentage End { get; private set; }

    public AmountAndPercentage Start { get; private set; }

    public YearAmountAndPercentage(AmountAndPercentage start, AmountAndPercentage end)
    {
        Start = start;
        End = end;

        Difference = new AmountAndPercentage(End.Amount - Start.Amount, End.Percentage - Start.Percentage);
    }
}