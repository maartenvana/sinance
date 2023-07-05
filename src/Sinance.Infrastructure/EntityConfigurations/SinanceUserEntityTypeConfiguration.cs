using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinance.Domain.Model;

namespace Sinance.Infrastructure.EntityConfigurations;

public class SinanceUserEntityTypeConfiguration : IEntityTypeConfiguration<SinanceUser>
{
    public void Configure(EntityTypeBuilder<SinanceUser> builder)
    {
        builder
            .ToTable("Users")
            .Property(x => x.Id)
            .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn)
            .ValueGeneratedOnAdd();

    }
}
