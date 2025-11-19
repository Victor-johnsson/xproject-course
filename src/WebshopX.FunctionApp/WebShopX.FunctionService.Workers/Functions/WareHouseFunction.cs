using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebShopX.FunctionService.Services;

namespace WebShopX.FunctionService.Api.Functions
{
    public class WareHouseFunction
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ITableStorageService _tableStorageService;
        private readonly ICrmService _crmService;
        private readonly ILogger<WareHouseFunction> _logger;

        public WareHouseFunction(
            ServiceBusClient serviceBusClient,
            ITableStorageService TableStorageService,
            ICrmService crmService,
            ILogger<WareHouseFunction> logger
        )
        {
            _serviceBusClient = serviceBusClient;
            _tableStorageService = TableStorageService;
            _crmService = crmService;
            _logger = logger;
        }

        [Function("WareHouseTimer")]
        //Cron : MIN, HOUR, DaysOfMonth, MONTH, Day of the Week (0-7, where both 0 and 7 are Sunday)
        public async Task WareHouseTimer(
            [TimerTrigger("*/5 * * * *", RunOnStartup = false)] TimerInfo myTimer
        )
        {
            try
            {
                var ordersToUpdate = _tableStorageService.Get5MinutesOldOrders();

                //TODO: What to send and where, should it be new topic or should functions that exist handle it.
                string topicName = "topicName";

                ServiceBusSender sender = _serviceBusClient.CreateSender(topicName);
                using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

                foreach (var order in ordersToUpdate)
                {
                    order.Status = "Picked and Ready for Transportation";
                    ServiceBusMessage message = new ServiceBusMessage(order.ToString());

                    if (messageBatch.TryAddMessage(message))
                        throw new Exception("Message  batch is full");

                    //TODO:
                    await _crmService.UpdateOrderStatus(order.Status, order.RowKey);
                }
                await sender.SendMessagesAsync(messageBatch);
            }
            catch (Exception e)
            {
                using (
                    _logger.BeginScope(
                        new Dictionary<string, object>
                        {
                            ["Service"] = "FunctionApp",
                            ["Function"] = nameof(WareHouseTimer),
                        }
                    )
                )
                {
                    _logger.LogError("Error {Error}", e);
                }
            }
        }
    }
}
