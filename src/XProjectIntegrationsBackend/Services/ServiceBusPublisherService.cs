using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace XProjectIntegrationsBackend.Services;

public class ServiceBusPublisherService
{
    private readonly ILogger<ServiceBusPublisherService> _logger;
    private readonly string _topicName;

    private readonly ServiceBusClient _serviceBusClient;

    public ServiceBusPublisherService(
        IConfiguration configuration,
        ILogger<ServiceBusPublisherService> logger,
        ServiceBusClient serviceBusClient
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration["ServiceBus:TopicName"]);
        _topicName = configuration["ServiceBus:TopicName"]!;
        _logger = logger;
        _serviceBusClient = serviceBusClient;
    }

    public async Task<int> GetMessageCountAsync()
    {
        var reciever = _serviceBusClient.CreateReceiver(_topicName, "sbts-order-created");

        try
        {
            var message = await reciever.PeekMessagesAsync(10);
            return message.Count;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Error creating message");
            throw;
        }
    }

    public async Task PublishMessageAsync<T>(T message)
    {
        var sender = _serviceBusClient.CreateSender(_topicName);

        string messageBody = JsonSerializer.Serialize(message);

        try
        {
            ServiceBusMessage serviceBusMessage = new(messageBody)
            {
                ContentType = "application/json",
                Subject = "OrderCreated",
            };

            serviceBusMessage.ApplicationProperties["MessageType"] = "OrderCreated";

            await sender.SendMessageAsync(serviceBusMessage);
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Error creating message");
            throw;
        }
    }
}
