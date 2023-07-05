namespace Sinance.Contracts.Transaction;

public class FindAccountTransactionsFilter
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? BankAccountId { get; set; }

    public int? CategoryId { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}