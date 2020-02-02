using System;
using System.Collections.Generic;
using System.Text;

namespace Sinance.Communication.Model.StandardReport.Yearly
{
    public class YearBalance
    {
        public decimal Start { get; private set; }
        public decimal End { get; private set; }

        public decimal Difference => End - Start;
    

        public YearBalance(decimal start, decimal end)
        {
            Start = start;
            End = end;
        }
    }
}
