using FluentValidation;
using Sinance.Application.Command.Transaction;

namespace Sinance.Application.Command.Validators;

public class SplitAccountTransactionCommandValidator : AbstractValidator<SplitAccountTransactionCommand>
{
    public SplitAccountTransactionCommandValidator()
    {
        RuleFor(x => x.SourceTransactionId).GreaterThan(0);
        
        // While this is validation kind of logic i'd have to query the database to get this done, undecided
        //RuleFor(x => x.NewTransactions).Must(x => x.Sum(x => x.Amount) == ).WithMessage("Total amount should be equal to source transaction");
    }
}
