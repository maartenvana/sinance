using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinance.Domain.Model;

namespace Sinance.Infrastructure.EntityConfigurations;

public class CategoryMappingEntityTypeConfiguration : IEntityTypeConfiguration<CategoryMapping>
{
    private readonly IUserIdProvider _userIdProvider;

    public CategoryMappingEntityTypeConfiguration(IUserIdProvider userIdProvider)
    {
        this._userIdProvider = userIdProvider;
    }
    public void Configure(EntityTypeBuilder<CategoryMapping> builder)
    {
        builder
            .ToTable("CategoryMapping")
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
            .HasOne(x => x.Category)
            .WithMany()
            .IsRequired();

        builder
            .Property(x => x.ColumnTypeId)
            .IsRequired();
        
        builder
            .Property(x => x.MatchValue)
            .HasMaxLength(200)
            .IsRequired();
    }
}
