using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace XProject.Integrations.CRM.EntityFramework.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? Status { get; set; }
        public string? PaymentId  { get; set; }
        public Customer? Customer { get; set; }
        public ICollection<OrderLine>? OrderLines { get; set; }
    }
}
