using Sinance.Application.Model;

namespace Sinance.Application.Command.Transaction
{
    public class SplitAccountTransactionCommand : IRequest<List<Domain.Model.AccountTransaction>>
    {
        [DataMember]
        public int SourceTransactionId { get; set; }

        [DataMember]
        public List<AccountTransactionCreationModel> NewTransactions { get; set; } = new List<AccountTransactionCreationModel>();

        public SplitAccountTransactionCommand()
        {

        }

        public SplitAccountTransactionCommand(int sourceTransactionId, List<AccountTransactionCreationModel> newTransactions)
        {
            SourceTransactionId = sourceTransactionId;
            NewTransactions = newTransactions;
        }
    }
}
