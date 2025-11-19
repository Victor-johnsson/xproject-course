using System.Text.Json;
using Azure.Data.Tables;
using Azure.Messaging.ServiceBus;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebShopX.FunctionService.Core.Models;
using WebShopX.FunctionService.Services;

namespace WebShopX.FunctionService.Api.Functions
{
    public class TransportFunction
    {
        private readonly ILogger<TransportFunction> _logger;
        private readonly ITableStorageService _tableStorageService;
        private readonly TableServiceClient _tableServiceClient;

        public TransportFunction(
            TableServiceClient tableServiceClient,
            ILogger<TransportFunction> logger,
            ITableStorageService tableStorageService
        )
        {
            _logger = logger;
            _tableStorageService = tableStorageService;
            _tableServiceClient = tableServiceClient;
        }

        [Function(nameof(ServiceBusTransportFunction))]
        public async Task ServiceBusTransportFunction(
            [ServiceBusTrigger(
                "%serviceBusTopicName%",
                "%serviceBusTransportSubscriptionName%",
                Connection = "serviceBus",
                IsSessionsEnabled = false
            )]
                ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions
        )
        {
            _logger.LogInformation($"Received message: {message.MessageId}");
            string messageBody = message.Body.ToString();
            try
            {
                Order? sentMessage = JsonSerializer.Deserialize<Order>(messageBody);

                if (string.IsNullOrEmpty(sentMessage?.PaymentId))
                {
                    _logger.LogError("Order status is missing.");
                    return;
                }

                TransportationEntity transportationEntityMessage = new TransportationEntity
                {
                    PartitionKey = "DHLTest4",
                    RowKey = sentMessage?.PaymentId ?? string.Empty,
                    Status = sentMessage?.Status ?? string.Empty,
                    CustomerName = sentMessage?.Customer?.Name ?? string.Empty,
                    CustomerEmail = sentMessage?.Customer?.Email ?? string.Empty,
                    CustomerAddress = sentMessage?.Customer?.Address ?? string.Empty,
                };

                if (sentMessage == null)
                {
                    _logger.LogError("Message deserialization failed. Skipping processing.");
                    return;
                }

                try
                {
                    await _tableStorageService.UpdateOrderStatus(transportationEntityMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating order status: {ex.Message}");
                    return;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Error deserializing message: {ex.Message}");
                return;
            }
        }
    }
}
