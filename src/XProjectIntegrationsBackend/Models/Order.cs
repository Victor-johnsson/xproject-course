using System.Text.Json.Serialization;

namespace XProjectIntegrationsBackend.Models
{
    public class Order
    {
        public string Status { get; set; } = string.Empty;
        public required string PaymentId { get; set; }
        public required Customer Customer { get; set; }
        public List<OrderLine> OrderLines { get; set; } = [];
    }
}
