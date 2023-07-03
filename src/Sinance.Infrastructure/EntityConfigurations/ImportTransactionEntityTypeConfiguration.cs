using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinance.Domain.Model;

namespace Sinance.Infrastructure.EntityConfigurations
{
    public class ImportTransactionEntityTypeConfiguration : IEntityTypeConfiguration<ImportTransaction>
    {
        private readonly IUserIdProvider _userIdProvider;

        public ImportTransactionEntityTypeConfiguration(IUserIdProvider userIdProvider)
        {
            this._userIdProvider = userIdProvider;
        }
        public void Configure(EntityTypeBuilder<ImportTransaction> builder)
        {
            builder
                .ToTable("ImportTransactions")
                .Property(x => x.Id)
                .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn)
                .ValueGeneratedOnAdd();

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

            builder.Property(x => x.AccountNumber).HasMaxLength(50);
            builder.Property(x => x.DestinationAccount).HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
            builder.Property(x => x.Date).IsRequired();
        }
    }
}
