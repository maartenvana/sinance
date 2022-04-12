using Sinance.Application.Queries;

namespace Sinance.Application.Command
{
    public class SplitAccountTransactionCommand : IRequest<List<int>>
    {
        [DataMember]
        public int SourceTransactionId { get; set; }

        [DataMember]
        public List<AccountTransactionViewModel> NewTransactions { get; set; } = new List<AccountTransactionViewModel>();

        public SplitAccountTransactionCommand()
        {

        }

        public SplitAccountTransactionCommand(int sourceTransactionId, List<AccountTransactionViewModel> newTransactions)
        {
            SourceTransactionId = sourceTransactionId;
            NewTransactions = newTransactions;
        }
    }
}
