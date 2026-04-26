using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderInventory.Persistence.Entities;

namespace OrderInventory.Persistence.Configurations;

public sealed class ProductEntityConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("PRODUCTS");
        builder.HasKey(x => x.ProductId).HasName("PK_PRODUCTS");

        builder.Property(x => x.ProductId).HasColumnName("PRODUCT_ID").HasColumnType("NUMBER(10)");
        builder.Property(x => x.CategoryId).HasColumnName("CATEGORY_ID").HasColumnType("NUMBER(10)");
        builder.Property(x => x.ProductCode).HasColumnName("PRODUCT_CODE").HasMaxLength(30).IsRequired();
        builder.Property(x => x.ProductName).HasColumnName("PRODUCT_NAME").HasMaxLength(200).IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnName("UNIT_PRICE").HasColumnType("NUMBER(12,2)");
        builder.Property(x => x.CurrentStockQty).HasColumnName("CURRENT_STOCK_QTY").HasColumnType("NUMBER(10)");
        builder.Property(x => x.SafetyStockQty).HasColumnName("SAFETY_STOCK_QTY").HasColumnType("NUMBER(10)");
        builder.Property(x => x.StatusCode).HasColumnName("STATUS_CODE").HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("CREATED_AT");
        builder.Property(x => x.UpdatedAt).HasColumnName("UPDATED_AT");

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .HasConstraintName("FK_PRODUCTS_CATEGORY");

        builder.HasIndex(x => x.ProductCode).IsUnique().HasDatabaseName("UK_PRODUCTS_CODE");
        builder.HasIndex(x => new { x.StatusCode, x.CategoryId }).HasDatabaseName("IDX_PRODUCTS_STATUS_CATEGORY");
    }
}