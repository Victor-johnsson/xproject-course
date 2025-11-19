using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using WebShopX.FunctionService.Core.Models;

namespace WebShopX.FunctionService.Services
{
    public interface ITableStorageService
    {
        List<TransportationEntity> Get5MinutesOldOrders();
        Task UpdateOrderStatus(TransportationEntity sentMessage);
    }

    public class TableStorageService : ITableStorageService
    {
        private readonly ILogger<TableStorageService> _logger;
        private readonly TableServiceClient _tableServiceClient;

        //TODO: Change to config
        private string _tableName = "TransportStatus";

        // private readonly SecretClient _secretClient;

        public TableStorageService(
            TableServiceClient tableServiceClient,
            ILogger<TableStorageService> logger
        )
        // IConfiguration configuration, SecretClient secretClient)
        {
            _tableServiceClient = tableServiceClient;
            _logger = logger;
            // _secretClient = secretClient;

            // Load secrets asynchronously without blocking constructor
            // Task.Run(() => LoadSecretsAsync()).Wait();
        }

        // private async Task LoadSecretsAsync()
        // {
        //     try
        //     {
        //         _logger.LogInformation("Loading secrets from Azure Key Vault...");
        //
        //         KeyVaultSecret? storageTableNameSecret = await _secretClient.GetSecretAsync("StorageTableName") ??
        //             throw new InvalidOperationException("StorageTableName not found in Key Vault.");
        //
        //         _tableName = storageTableNameSecret.Value ??
        //             throw new InvalidOperationException("storageTableName is empty in Key Vault.");
        //
        //         _logger.LogInformation("Secrets successfully loaded.");
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error loading secrets from Azure Key Vault.");
        //         throw;
        //     }
        // }
        //
        public List<TransportationEntity> Get5MinutesOldOrders()
        {
            TableClient tableClient = _tableServiceClient.GetTableClient(_tableName);
            DateTimeOffset fiveMinutesAgo = DateTimeOffset.UtcNow.AddMinutes(-5);
            string filter = $"Timestamp le datetime'{fiveMinutesAgo:O}'";

            Pageable<TableEntity> res = tableClient.Query<TableEntity>(filter);

            var list = new List<TransportationEntity>();
            foreach (TableEntity item in res)
            {
                //Todo: Map to object when structire is cleaer, and set correct return type
                //list.Add(new TransportationEntity());
            }

            return list;
        }

        public async Task UpdateOrderStatus(TransportationEntity sentMessage)
        {
            try
            {
                TableClient tableClient = _tableServiceClient.GetTableClient(_tableName);

                var entity = new TableEntity(sentMessage.PartitionKey, sentMessage.RowKey)
                {
                    { "Status", sentMessage.Status },
                    { "CustomerName", sentMessage.CustomerName },
                    { "CustomerEmail", sentMessage.CustomerEmail },
                    { "CustomerAddress", sentMessage.CustomerAddress },
                    { "LastUpdated", DateTime.UtcNow.ToString("o") },
                };

                await tableClient.UpsertEntityAsync(entity);

                _logger.LogInformation(
                    $"Updated TransportStatus for {sentMessage.RowKey} to {sentMessage.Status}"
                );
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"Error updating entity: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
