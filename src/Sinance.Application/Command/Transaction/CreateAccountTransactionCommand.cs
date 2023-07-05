using Sinance.Application.Model;
using Sinance.Domain.Model;

namespace Sinance.Application.Command.Transaction;

public class CreateAccountTransactionCommand : IRequest<AccountTransaction>
{
    public AccountTransactionCreationModel CreationModel { get; set; }
    public int UserId { get; set; }
}
