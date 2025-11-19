using System.Text.Json;
using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using XProjectIntegrationsBackend.Models;

namespace XProjectIntegrationsBackend.Services;

public class EventGridService
{
    private readonly string _eventGridTopicEndpoint;
    private readonly string _eventGridTopicKey;
    private readonly ILogger<EventGridService> _logger;

    public EventGridService(IConfiguration configuration, ILogger<EventGridService> logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration["EventGrid:eventGridTopicEndpoint"]);
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration["EventGrid:EventGridTopicKey"]);
        _eventGridTopicEndpoint = configuration["EventGrid:eventGridTopicEndpoint"]!;
        _eventGridTopicKey = configuration["EventGrid:EventGridTopicKey"]!;
        _logger = logger;
    }

    public async Task PublishOrderEventAsync(Order order)
    {
        try
        {
            var eventGridClient = new EventGridPublisherClient(
                new Uri(_eventGridTopicEndpoint),
                new AzureKeyCredential(_eventGridTopicKey)
            );

            var cloudEvent = new CloudEvent(
                source: $"orders/{order.PaymentId}",
                type: "OrderCreated",
                jsonSerializableData: order
            )
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow,
            };

            _logger.LogDebug("CloudEvent {Event}", JsonSerializer.Serialize(cloudEvent));

            await eventGridClient.SendEventAsync(cloudEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send event to Event Grid:");
            throw;
        }
    }
}
