using Azure.Identity;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebShopX.FunctionService.Services;

var builder = FunctionsApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.ConfigureFunctionsWebApplication();

// .ConfigureOpenApi()
// builder.ConfigureServices((context, services) => { });

var config = builder.Configuration;
config
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
var credential = new DefaultAzureCredential();

// var keyVaultUrl = config["UrlKeyVault"] ?? throw new Exception("No KeyVaultUrl in config.");
// SecretClient secretClient = new(new Uri(keyVaultUrl), credential);
// builder.Services.AddSingleton(secretClient);
builder.AddRedisClient("redis");
builder.AddAzureServiceBusClient("serviceBus");

// builder.Services.AddAzureClients(clientsBuilder =>
// {
//     clientsBuilder.UseCredential(credential);
//
//     // Fetch the Service Bus Connection String from Key Vault (not appsettings.json)
//     var serviceBusConnectionStringSecret = secretClient.GetSecret("ServiceBusConnectionString");
//     var serviceBusConnectionString =
//         serviceBusConnectionStringSecret.Value.Value
//         ?? throw new Exception("No ServiceBusConnectionString found in Key Vault.");
//
//     clientsBuilder
//         .AddServiceBusClient(serviceBusConnectionString)
//         .WithName<
//             Azure.Messaging.ServiceBus.ServiceBusClient,
//             Azure.Messaging.ServiceBus.ServiceBusClientOptions
//         >(WebShopX.FunctionService.Core.Constants.SERVICEBUSCLIENTNAME)
//         .ConfigureOptions(options =>
//         {
//             options.RetryOptions.Mode = Azure.Messaging.ServiceBus.ServiceBusRetryMode.Exponential;
//             options.RetryOptions.Delay = TimeSpan.FromMilliseconds(50);
//             options.RetryOptions.MaxDelay = TimeSpan.FromSeconds(5);
//             options.RetryOptions.MaxRetries = 8;
//         });
// });

// Fetch the StorageAccount Connection String from Key Vault (not appsettings.json)
// var storagetableServiceClientSecret = secretClient.GetSecret("StorageAccountConnectionString");
// var storageTableServiceClientConnectionString =
//     storagetableServiceClientSecret.Value.Value
//     ?? throw new Exception("No StorageAccountConnectionString in config.");
//
// //TODO: Add TableStorage Apphost
// builder.Services.AddSingleton(new TableServiceClient(storageTableServiceClientConnectionString));
//
builder.AddAzureTableServiceClient("tables");
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

builder.Services.AddTransient<ITableStorageService, TableStorageService>();

builder.Services.AddHttpClient<IPimService, PimService>(e =>
    e.BaseAddress = new Uri("https+http://productCatalog")
);

builder.Services.AddHttpClient<ICrmService, CrmService>(client =>
{
    client.BaseAddress = new Uri("https+http://crmApplication");
});
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

builder.Build().Run();
