using FluentValidation;
using Sinance.Application.Command.Transaction;

namespace Sinance.Application.Command.Validators
{
    public class CreateAccountTransactionCommandValidator : AbstractValidator<CreateAccountTransactionCommand>
    {
        public CreateAccountTransactionCommandValidator()
        {
            RuleFor(c => c.UserId).NotEmpty();
        }
    }
}
