using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinance.Domain.Model;

namespace Sinance.Infrastructure.EntityConfigurations;

public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
    private readonly IUserIdProvider _userIdProvider;

    public CategoryEntityTypeConfiguration(IUserIdProvider userIdProvider)
    {
        this._userIdProvider = userIdProvider;
    }

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder
            .ToTable("Category")
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
            .HasIndex(x => new { x.ShortName, x.UserId })
            .IsUnique(true);
        builder
            .HasIndex(x => new { x.Name, x.UserId })
            .IsUnique(true);
    }
}
