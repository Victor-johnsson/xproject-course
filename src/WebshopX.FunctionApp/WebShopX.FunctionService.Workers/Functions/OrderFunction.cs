using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebShopX.FunctionService.Core.Models;
using WebShopX.FunctionService.Services;

namespace WebShopX.FunctionService.Api.Functions
{
    public class OrderFunction
    {
        private readonly ICrmService _crmService;
        private readonly ILogger<OrderFunction> _logger;

        public OrderFunction(ICrmService crmService, ILogger<OrderFunction> logger)
        {
            _crmService = crmService;
            _logger = logger;
        }

        [Function(nameof(ServiceBusOrderFunction))]
        // public async Task ServiceBusOrderFunction([ServiceBusTrigger("%serviceBusTopicName%","%serviceBusSubscriptionName%", Connection = "serviceBusConnectionString", IsSessionsEnabled = false)]
        public async Task ServiceBusOrderFunction(
            [ServiceBusTrigger(
                "sbt-xproject-integrations",
                "sbts-order-created",
                Connection = "serviceBus",
                IsSessionsEnabled = false
            )]
                ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions
        )
        {
            try
            {
                _logger.LogInformation("Received Order from ServiceBus");
                // Convert the message body to JSON string
                string jsonString = Encoding.UTF8.GetString(message.Body);

                var order = JsonSerializer.Deserialize<Order>(jsonString);

                await _crmService.CreateOrder(order);
            }
            catch (Exception e)
            {
                using (
                    _logger.BeginScope(
                        new Dictionary<string, object>
                        {
                            ["Service"] = "FunctionApp",
                            ["Function"] = nameof(OrderFunction),
                        }
                    )
                )
                {
                    _logger.LogError(e.Message, e);
                }
            }
        }
    }
}
