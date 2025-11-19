namespace WebShopX.FunctionService.Core.Models
{
    public class Order
    {
        public required string Status { get; set; }
        public required string PaymentId { get; set; }
        public required Customer Customer { get; set; }
        public required List<OrderLine> OrderLines { get; set; }
    }

    public class Customer
    {
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string Email { get; set; }
    }

    public class OrderLine
    {
        public required string ProductId { get; set; }
        public required int ItemCount { get; set; }
    }
}
