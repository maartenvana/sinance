using FluentValidation;

namespace Sinance.Application.Command.Validators
{
    public class SplitAccountTransactionCommandValidator : AbstractValidator<SplitAccountTransactionCommand>
    {
        public SplitAccountTransactionCommandValidator(decimal sourceTransactionAmount)
        {
            RuleFor(x => x.SourceTransactionId).GreaterThan(0);

            RuleFor(x => x.NewTransactions).Must(x => x.Sum(x => x.Amount) == sourceTransactionAmount).WithMessage("Total amount should be equal to source transaction");
        }
    }
}
