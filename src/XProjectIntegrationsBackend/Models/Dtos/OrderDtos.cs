namespace XProjectIntegrationsBackend.Models.Dtos;

public class OrderDtos
{
    public record CreateOrderRequest(
        List<OrderLineRequest> OrderLines,
        CustomerRequest Customer,
        int PaymentId
    );

    public record OrderLineRequest(
        Guid ProductId,
        int ItemCount,
        int CurrentStockQuantity
    );

    public record CustomerRequest(
        string Name,
        string Address,
        string Email
    );
}
