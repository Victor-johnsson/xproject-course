using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace XProject.Integrations.CRM.EntityFramework.Entities
{
    public class OrderLine
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string? ProdRef { get; set; } // External, product stored in CosmosDB 
        public Order? Order { get; set; }
        public int ItemCount { get; set; }
    }
}
