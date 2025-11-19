using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebShopX.FunctionService.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WebShopX.FunctionService.Core.Models;
using Azure.Messaging;

namespace WebShopX.FunctionService.Api.Functions
{
    public class ProductFunction
    {
        private readonly ILogger<ProductFunction> _logger;
        private readonly IPimService _pimService;

        public ProductFunction(ILogger<ProductFunction> logger, IPimService pimService)
        {
            _logger = logger;
            _pimService = pimService;
        }

        [Function("UpdateProductCount")]
        public async Task UpdateProductCount([EventGridTrigger] CloudEvent cloudEvent)
        {
            try
            {
                _logger.LogInformation($"Received Event: {cloudEvent.Data}");

                string rawData = cloudEvent.Data.ToString();

                if (!string.IsNullOrEmpty(rawData) && rawData.StartsWith("\"") && rawData.EndsWith("\""))
                {
                    rawData = JsonSerializer.Deserialize<string>(rawData) ?? string.Empty;
                }

                var eventData = JsonSerializer.Deserialize<Order>(rawData);

                if (eventData == null || string.IsNullOrEmpty(eventData.PaymentId))
                {
                    throw new Exception("Invalid event data: Missing ProductId or Stock.");
                }

                if (eventData?.OrderLines?.Count > 0)
                {
                    var tasks = new List<Task>();
                    foreach (var orderLine in eventData.OrderLines)
                    {
                        if (string.IsNullOrEmpty(orderLine.ProductId) || orderLine.ItemCount < 0)
                        {
                            throw new Exception("Invalid OrderLine: Missing ProductId or invalid ItemCount.");
                        }

                        _logger.LogInformation($"Updating ProductId: {orderLine.ProductId} with ItemCount: {orderLine.ItemCount}");

                        tasks.Add(_pimService.UpdateStock(orderLine.ProductId, orderLine.ItemCount));
                    }
                    await Task.WhenAll(tasks);
                }

            }
            catch (Exception e)
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["Service"] = "FunctionApp",
                    ["Function"] = nameof(UpdateProductCount),
                }))
                {
                    _logger.LogError(e.Message, e);
                }
            }
        }
    }
}
