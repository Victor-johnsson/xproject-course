using System.Text.Json.Serialization;

namespace XProjectIntegrationsBackend.Models
{
    public class Customer
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
    }
}
