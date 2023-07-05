using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinance.Domain.Model;

namespace Sinance.Infrastructure.EntityConfigurations;

public class AccountTransactionEntityTypeConfiguration : IEntityTypeConfiguration<AccountTransaction>
{
    private readonly IUserIdProvider _userIdProvider;

    public AccountTransactionEntityTypeConfiguration(IUserIdProvider userIdProvider)
    {
        this._userIdProvider = userIdProvider;
    }
    public void Configure(EntityTypeBuilder<AccountTransaction> builder)
    {
        builder
            .ToTable("Transactions")
            .Property(x => x.Id)
            .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();

        builder
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(nameof(AccountTransaction.CategoryId));

        builder
            .HasQueryFilter(x => x.UserId == _userIdProvider.GetCurrentUserId());

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(nameof(UserEntity.UserId))
            .IsRequired();

        builder
            .HasOne(x => x.BankAccount)
            .WithMany()
            .HasForeignKey(nameof(AccountTransaction.BankAccountId));

        builder
            .HasOne(x => x.ImportTransaction)
            .WithMany()
            .HasForeignKey(nameof(AccountTransaction.ImportTransactionId));

        builder.Property(x => x.AccountNumber).HasMaxLength(50);
        builder.Property(x => x.DestinationAccount).HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Date).IsRequired();
    }
}
