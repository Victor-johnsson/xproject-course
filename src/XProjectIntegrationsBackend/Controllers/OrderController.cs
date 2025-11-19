using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XProjectIntegrationsBackend.Models;
using XProjectIntegrationsBackend.Services;
using static XProjectIntegrationsBackend.Models.Dtos.OrderDtos;

namespace XProjectIntegrationsBackend.Controllers;

[Route("/orders")]
[ApiController]
public class OrderController : ControllerBase
{
    IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IResult> CreateOrder(CreateOrderRequest createOrderRequest)
    {
        return await _orderService.CreateOrderAsync(createOrderRequest);
    }

    [HttpGet]
    public async Task<IResult> GetMessageCount()
    {
        return await _orderService.GetMessageCountAsync();
    }
}

public interface IOrderService
{
    Task<IResult> CreateOrderAsync(CreateOrderRequest createOrderRequest);
    Task<IResult> GetMessageCountAsync();
}

public class OrderService : IOrderService
{
    private readonly ServiceBusPublisherService _serviceBusPublisherService;

    // private readonly EventGridService _eventGridService;
    private readonly ILogger<OrderController> _logger;

    public OrderService(
        ILogger<OrderController> logger,
        ServiceBusPublisherService serviceBusPublisherService
    // EventGridService eventGridService
    )
    {
        _logger = logger;
        _serviceBusPublisherService = serviceBusPublisherService;
        // _eventGridService = eventGridService;
    }

    public async Task<IResult> GetMessageCountAsync()
    {
        try
        {
            var count = await _serviceBusPublisherService.GetMessageCountAsync();
            _logger.LogInformation("Order published to Service Bus");

            // TODO: Publish order to Event Grid
            // await _eventGridService.PublishOrderEventAsync(order);
            // _logger.LogInformation("Order event published to Event Grid");

            return TypedResults.Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return TypedResults.Problem(
                new()
                {
                    Status = 500,
                    Title = "Internal Server Error",
                    Detail = $"Internal Server Error: {ex.Message}",
                }
            );
        }
    }

    public async Task<IResult> CreateOrderAsync(CreateOrderRequest createOrderRequest)
    {
        try
        {
            var order = new Order
            {
                Status = "Pending",
                PaymentId = createOrderRequest.PaymentId.ToString(),
                Customer = new Customer
                {
                    Name = createOrderRequest.Customer.Name,
                    Address = createOrderRequest.Customer.Address,
                    Email = createOrderRequest.Customer.Email,
                },
                OrderLines =
                [
                    .. createOrderRequest.OrderLines.Select(x => new OrderLine
                    {
                        ItemCount = x.ItemCount,
                        ProductId = x.ProductId,
                    }),
                ],
            };

            await _serviceBusPublisherService.PublishMessageAsync(order);
            _logger.LogInformation("Order published to Service Bus");

            // TODO: Publish order to Event Grid
            // await _eventGridService.PublishOrderEventAsync(order);
            // _logger.LogInformation("Order event published to Event Grid");

            return TypedResults.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return TypedResults.Problem(
                new()
                {
                    Status = 500,
                    Title = "Internal Server Error",
                    Detail = $"Internal Server Error: {ex.Message}",
                }
            );
        }
    }
}
