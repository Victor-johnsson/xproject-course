using System.Text.Json.Serialization;

namespace XProjectIntegrationsBackend.Models
{
    public class OrderLine
    {
        public required Guid ProductId { get; set; }
        public required int ItemCount { get; set; }
    }
}
