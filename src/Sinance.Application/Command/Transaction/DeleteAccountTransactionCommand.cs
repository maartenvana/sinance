namespace Sinance.Application.Command.Transaction;

public class DeleteAccountTransactionCommand : IRequest
{
    public int UserId { get; set; }
    public int TransactionId { get; set; }
}
