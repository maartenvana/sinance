using Sinance.Application.Model;
using Sinance.Domain.Model;

namespace Sinance.Application.Command.Transaction
{
    public class UpdateAccountTransactionCommand : IRequest<AccountTransaction>
    {
        public int UserId { get; set; }
        public int TransactionId { get; set; }
        public AccountTransactionUpdateModel UpdateModel { get; set; }
    }
}
