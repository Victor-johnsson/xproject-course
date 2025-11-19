namespace Models;

public class Product
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public string PartitionKey { get; set; } = "products";
}
