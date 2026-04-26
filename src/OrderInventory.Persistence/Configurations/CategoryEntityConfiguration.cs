using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderInventory.Persistence.Entities;

namespace OrderInventory.Persistence.Configurations;

public sealed class CategoryEntityConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.ToTable("CATEGORIES");
        builder.HasKey(x => x.CategoryId).HasName("PK_CATEGORIES");

        builder.Property(x => x.CategoryId).HasColumnName("CATEGORY_ID").HasColumnType("NUMBER(10)");
        builder.Property(x => x.CategoryCode).HasColumnName("CATEGORY_CODE").HasMaxLength(30).IsRequired();
        builder.Property(x => x.CategoryName).HasColumnName("CATEGORY_NAME").HasMaxLength(100).IsRequired();
        builder.Property(x => x.UseYn).HasColumnName("USE_YN").HasMaxLength(1).HasDefaultValue("Y").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("CREATED_AT");
        builder.Property(x => x.UpdatedAt).HasColumnName("UPDATED_AT");

        builder.HasIndex(x => x.CategoryCode).IsUnique().HasDatabaseName("UK_CATEGORIES_CODE");
    }
}