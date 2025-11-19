using System.Text.Json.Serialization;

namespace XProject.Integrations.CRM.Models.RequestModels
{
    public class CreateOrderModel
    {
        public string? PaymentId { get; set; }
        public CustomerReqModel? Customer { get; set; }
        public List<OrderLinesReqModel>? OrderLines { get; set; }
    }

    public class CustomerReqModel
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
    }

    public class OrderLinesReqModel
    {
        public string? ProductId { get; set; }
        public int ItemCount { get; set; }
    }
}
