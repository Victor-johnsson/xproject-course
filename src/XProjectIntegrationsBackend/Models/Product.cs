using System.Text.Json.Serialization;

namespace XProjectIntegrationsBackend.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int? Price { get; set; }
        public int? Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageBase64 { get; set; }
    }
}
