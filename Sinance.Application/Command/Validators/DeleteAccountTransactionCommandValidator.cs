using FluentValidation;
using Sinance.Application.Command.Transaction;

namespace Sinance.Application.Command.Validators
{
    public class DeleteAccountTransactionCommandValidator : AbstractValidator<DeleteAccountTransactionCommand>
    {
        public DeleteAccountTransactionCommandValidator()
        {
            RuleFor(c => c.TransactionId).NotEmpty();
            RuleFor(c => c.UserId).NotEmpty();
        }
    }
}
