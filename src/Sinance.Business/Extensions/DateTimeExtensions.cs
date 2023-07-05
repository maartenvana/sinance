using System;

namespace Sinance.Business.Extensions;

public static class DateTimeExtensions
{
    public static int GetMonthsBetween(this DateTime startDate, DateTime endDate) => 
        ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;

    public static DateTime BeginningOfMonth(this DateTime date) =>
        new DateTime(date.Year, date.Month, 1).Date;

    public static DateTime EndOfTheMonth(this DateTime date) =>
        new DateTime(date.Year, date.Month, 1, 0, 0, 0, 0).AddMonths(1).AddMilliseconds(-1);
}
