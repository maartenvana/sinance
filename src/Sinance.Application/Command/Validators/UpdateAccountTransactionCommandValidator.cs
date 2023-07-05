using FluentValidation;
using Sinance.Application.Command.Transaction;

namespace Sinance.Application.Command.Validators;

public class UpdateAccountTransactionCommandValidator : AbstractValidator<UpdateAccountTransactionCommand>
{
    public UpdateAccountTransactionCommandValidator()
    {
        RuleFor(c => c.TransactionId).NotEmpty();
        RuleFor(c => c.UserId).NotEmpty();

        RuleFor(c => c.UpdateModel).NotNull();
    }
}
