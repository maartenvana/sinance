using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinance.Domain.Model;

namespace Sinance.Infrastructure.EntityConfigurations
{
    public class AccountEntityTypeConfiguration : IEntityTypeConfiguration<Account>
    {
        private readonly IUserIdProvider _userIdProvider;

        public AccountEntityTypeConfiguration(IUserIdProvider userIdProvider)
        {
            this._userIdProvider = userIdProvider;
        }

        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder
                .ToTable("BankAccount")
                .Property(x => x.Id)
                .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn)
                .ValueGeneratedOnAdd();

            builder
                .HasQueryFilter(x => x.UserId == _userIdProvider.GetCurrentUserId());

            builder
                .HasOne(x => x.User)
                .WithMany()
                .IsRequired();

            builder
                .Property(x => x.Name)
                .IsRequired();

            builder
                .HasIndex(x => new { x.Name, x.UserId })
                .IsUnique(true);
        }
    }
}