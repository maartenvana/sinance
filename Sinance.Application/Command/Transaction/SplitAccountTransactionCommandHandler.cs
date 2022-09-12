using Serilog;
using Sinance.Application.Command.Validators;
using Sinance.Application.Model;
using Sinance.Domain.Model;
using Sinance.Infrastructure;

namespace Sinance.Application.Command.Transaction
{
    public class SplitAccountTransactionCommandHandler : IRequestHandler<SplitAccountTransactionCommand, List<AccountTransaction>>
    {
        private readonly SinanceContext context;

        public SplitAccountTransactionCommandHandler(SinanceContext context)
        {
            this.context = context;
        }

        public async Task<List<AccountTransaction>> Handle(SplitAccountTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var sourceTransaction = context.Transactions.Single(x => x.Id == request.SourceTransactionId);

                var newTransactions = request.NewTransactions.Select(newTransaction => CreateNewTransaction(newTransaction, sourceTransaction)).ToList();

                context.Transactions.AddRange(newTransactions);
                context.Transactions.Remove(sourceTransaction);

                await context.SaveChangesAsync(cancellationToken);

                return newTransactions.ToList();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Exception during handle for SplitTransactionCommand");

                context.RollbackTransaction();

                throw;
            }
        }

        private static AccountTransaction CreateNewTransaction(AccountTransactionCreationModel newTransaction, AccountTransaction sourceTransaction)
        {
            return new AccountTransaction(
                userId: sourceTransaction.UserId,
                bankAccountId: newTransaction.BankAccountId,
                date: newTransaction.Date,
                name: newTransaction.Name,
                description: newTransaction.Description,
                amount: newTransaction.Amount,
                categoryId: newTransaction.CategoryId,
                account: newTransaction.SourceAccountNumber,
                destinationAcount: newTransaction.DestinationAccountNumber,
                importTransactionId: sourceTransaction.ImportTransactionId);
        }
    }
}
