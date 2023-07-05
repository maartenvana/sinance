using System;

namespace Sinance.Web.Model;

public class MonthYearSelectionModel
{
    public string Action { get; set; }

    public string Controller { get; set; }

    public DateTime CurrentDate { get; set; }
}