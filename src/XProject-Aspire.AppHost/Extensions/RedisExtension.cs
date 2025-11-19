using Aspire.Hosting.Azure;
using Azure.Core;
using Azure.Provisioning.CosmosDB;
using Azure.Provisioning.Redis;
using Azure.Provisioning.ServiceBus;
using Azure.Provisioning.Sql;
using Azure.Provisioning.Storage;

namespace Extensions;

public static class InfraExtensions
{

    public static IResourceBuilder<AzureRedisCacheResource> ConfigureRedisInfra(this IResourceBuilder<AzureRedisCacheResource>? redis)
    {

        ArgumentNullException.ThrowIfNull(redis);
        return redis.ConfigureInfrastructure(infra =>
        {
            var redisc = infra
                .GetProvisionableResources()
                .OfType<Azure.Provisioning.Redis.RedisResource>()
                .Single();

            redisc.Sku = new()
            {
                Family = RedisSkuFamily.BasicOrStandard,
                Name = RedisSkuName.Basic,
                Capacity = 0,
            };
        });
    }


    public static IResourceBuilder<AzureServiceBusResource> ConfigureServiceBusInfra(this IResourceBuilder<AzureServiceBusResource>? servicebus)
    {
        ArgumentNullException.ThrowIfNull(servicebus);

        return servicebus.ConfigureInfrastructure(infra =>
              {
                  var serviceBusNamespace = infra
                      .GetProvisionableResources()
                      .OfType<ServiceBusNamespace>()
                      .Single();
                  serviceBusNamespace.Sku = new ServiceBusSku() { Name = ServiceBusSkuName.Standard };
              });
    }



    public static IResourceBuilder<AzureStorageResource> ConfigureStorageInfra(this IResourceBuilder<AzureStorageResource>? storage)
    {
        ArgumentNullException.ThrowIfNull(storage);
        return storage.ConfigureInfrastructure(infra =>
        {
            var storageConf = infra.GetProvisionableResources().OfType<StorageAccount>().Single();
            storageConf.Sku = new StorageSku() { Name = StorageSkuName.StandardLrs };
            storageConf.AllowBlobPublicAccess = true;

        });
    }



    public static IResourceBuilder<AzureSqlServerResource> ConfigureAzureSqlInfra(this IResourceBuilder<AzureSqlServerResource>? sql)
    {
        ArgumentNullException.ThrowIfNull(sql);
        return sql.ConfigureInfrastructure(infra =>
        {
            var azSqlConfig = infra.GetProvisionableResources().OfType<SqlDatabase>().Single();
            azSqlConfig.Sku = new SqlSku() { Name = "GP_S_Gen5_1", Tier = "GeneralPurpose" };
        });
    }


    public static IResourceBuilder<AzureCosmosDBResource> ConfigureAzureCosmosDbInfra(this IResourceBuilder<AzureCosmosDBResource> cosmos)
    {
        return cosmos
            .ConfigureInfrastructure(infra =>
            {
                var cosmosConf = infra.GetProvisionableResources().OfType<CosmosDBAccount>().Single();
            });
    }

}
