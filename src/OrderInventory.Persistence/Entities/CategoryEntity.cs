namespace OrderInventory.Persistence.Entities;

public sealed class CategoryEntity
{
    public int CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string UseYn { get; set; } = "Y";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
}