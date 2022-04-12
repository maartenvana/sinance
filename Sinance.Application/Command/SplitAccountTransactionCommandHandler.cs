using Serilog;
using Sinance.Application.Command.Validators;
using Sinance.Application.Queries;
using Sinance.Domain.Model;
using Sinance.Infrastructure;

namespace Sinance.Application.Command
{
    public class SplitAccountTransactionCommandHandler : IRequestHandler<SplitAccountTransactionCommand, List<int>>
    {
        private readonly SinanceContext context;

        public SplitAccountTransactionCommandHandler(SinanceContext context)
        {
            this.context = context;
        }

        public async Task<List<int>> Handle(SplitAccountTransactionCommand request, CancellationToken cancellationToken)
        {
            var transaction = await context.BeginTransactionAsync();
            try
            {
                var sourceTransaction = context.Transactions.Single(x => x.Id == request.SourceTransactionId);

                var validator = new SplitAccountTransactionCommandValidator(sourceTransaction.Amount);
                validator.Validate(request);

                var newTransactions = request.NewTransactions.Select(x => CreateNewTransaction(x, sourceTransaction)).ToList();

                context.Transactions.AddRange(newTransactions);
                context.Transactions.Remove(sourceTransaction);

                await context.SaveEntitiesAsync(cancellationToken);
                await context.CommitTransactionAsync(transaction);

                return newTransactions.Select(x => x.Id).ToList();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Exception during handle for SplitTransactionCommand");

                context.RollbackTransaction();

                throw;
            }
        }

        private static AccountTransaction CreateNewTransaction(AccountTransactionViewModel x, AccountTransaction sourceTransaction)
        {
            return new AccountTransaction(
                                bankAccountId: sourceTransaction.BankAccountId,
                                date: x.Date,
                                name: x.Name,
                                description: x.Description,
                                amount: x.Amount,
                                categoryId: x.CategoryId,
                                account: x.SourceAccountNumber,
                                destinationAcount: x.DestinationAccountNumber,
                                importTransactionId: sourceTransaction.ImportTransactionId);
        }
    }
}
