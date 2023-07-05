using Sinance.Application.Model;
using Sinance.Domain.Model;
using Sinance.Infrastructure;

namespace Sinance.Application.Command.Transaction;

public class CreateAccountTransactionCommandHandler : IRequestHandler<CreateAccountTransactionCommand, AccountTransaction>
{
    private readonly SinanceContext context;

    public CreateAccountTransactionCommandHandler(SinanceContext context)
    {
        this.context = context;
    }

    public async Task<AccountTransaction> Handle(CreateAccountTransactionCommand request, CancellationToken cancellationToken)
    {
        var accountTransaction = CreateNewAccountTransactionFromRequest(request.CreationModel, request.UserId);

        context.Transactions.Add(accountTransaction);

        await context.SaveChangesAsync(cancellationToken);

        return accountTransaction;
    }

    private static AccountTransaction CreateNewAccountTransactionFromRequest(AccountTransactionCreationModel model, int userId) => new(userId: userId,
        bankAccountId: model.BankAccountId,
        date: model.Date,
        name: model.Name,
        description: model.Description,
        amount: model.Amount,
        categoryId: model.CategoryId,
        account: model.SourceAccountNumber,
        destinationAcount: model.DestinationAccountNumber,
        importTransactionId: null);
}