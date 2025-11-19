namespace XProjectIntegrationsBackend.Models
{
    public class CartItem
    {
        public required string ProductId { get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required double Price { get; set; }
    }
}
