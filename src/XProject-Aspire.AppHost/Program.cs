using Aspire.Hosting.Yarp.Transforms;
using Azure.Provisioning.Storage;
using Extensions;


var builder = DistributedApplication.CreateBuilder(args);

// ======== ENVIRONMENT & PARAMETERS ========
var env = builder.AddAzureAppServiceEnvironment("xproj-environment");

// ======== AZURE SERVICES ========
// Redis Cache
var redis = builder
    .AddAzureRedis("redis")
    .WithAccessKeyAuthentication()
    .ConfigureRedisInfra();

// Service Bus with Topics and Subscriptions
var servicebus = builder.AddAzureServiceBus("serviceBus");
var topic = servicebus.AddServiceBusTopic("topic", "sbt-xproject-integrations");
topic.AddServiceBusSubscription("logic-xproject-integrations");
topic.AddServiceBusSubscription("sbts-order-created");
topic.AddServiceBusSubscription("sbts-Transport-booking");
servicebus.ConfigureServiceBusInfra();

// Storage Account Resources
var storage = builder.AddAzureStorage("storage");
var blobs = storage.AddBlobs("blobs");
var tables = storage.AddTables("tables");
var images = storage.AddBlobContainer("productimages", "blobs");
storage.ConfigureStorageInfra();
string storageAccResourceId = "";
var test = storage.ConfigureInfrastructure((infra) =>
{
    var storageConf = infra.GetProvisionableResources().OfType<StorageAccount>().Single();

    storageConf.AllowBlobPublicAccess = true;
    storageAccResourceId = storageConf.Id.ToString();
});


var paramete = builder.AddParameter("storageAccountId", storageAccResourceId);
// ======== DATABASES ========
// CosmosDB for Product Information
var cosmos = builder
    .AddAzureCosmosDB("cosmospim")
    .ConfigureAzureCosmosDbInfra();

var productsDatabase = cosmos.AddCosmosDatabase("productsCosmosDb", "productsDatabase");
var productsDbContainer = productsDatabase.AddContainer("Products", "/PartitionKey");

// SQL Server for Customer Database
var customerDatabase = builder
    .AddAzureSqlServer("crmsql")
    .ConfigureAzureSqlInfra()
    .AddDatabase("crm");

// Postgres for Payments Database
var paymentsDatabase = builder
    .AddAzurePostgresFlexibleServer("paymentsDatabase")
    .AddDatabase("payments");

// ======== AZURE FUNCTIONS ========
var functions = builder.AddAzureFunctionsProject<Projects.WebShopX_FunctionService_Workers>("functions");

// ======== PROJECTS/SERVICES ========
// Product Catalog Service
var productCatalog = builder
    .AddProject<Projects.integration_pim_ApiService>("productCatalog")
    .WithExternalHttpEndpoints()
    .WithReference(productsDatabase)
    .WaitFor(productsDatabase)
    .WaitFor(productsDbContainer);

// CRM Application
var crm = builder
    .AddProject<Projects.XProject_Integrations_CRM>("crmApplication")
    .WithExternalHttpEndpoints()
    .WithReference(customerDatabase)
    .WaitFor(customerDatabase);

// Functions Service
functions
    .WithExternalHttpEndpoints()
    .WithReference(productCatalog)
    .WaitFor(productCatalog)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(crm)
    .WaitFor(crm)
    .WithReference(servicebus)
    .WaitFor(servicebus)
    .WithReference(tables)
    .WaitFor(tables);
// Payments Service
var paymentsService = builder
    .AddProject<Projects.integration_psp_ApiService>("paymentsService")
    .WithExternalHttpEndpoints()
    .WithReference(paymentsDatabase);

// Backend Service
var backend = builder
    .AddProject<Projects.XProjectIntegrationsBackend>("backend")
    .WithExternalHttpEndpoints()
    .WithReference(redis)
    .WithReference(servicebus)
    .WithReference(blobs)
    .WithReference(productCatalog)
    .WithReference(paymentsService)
    .WaitFor(redis)
    .WaitFor(servicebus)
    .WaitFor(paymentsService)
    .WaitFor(productCatalog);

// Frontend Application
var frontend = builder
    .AddViteApp("frontend", "../XProject.Integrations.Frontend")
    .WaitFor(backend);

var yarp = builder.AddYarp("yarp")
                  .WithExternalHttpEndpoints()
                  .WithConfiguration(c =>
                  {
                      c.AddRoute("/api/{**catch-all}", backend)
                      .WithTransformPathRemovePrefix("/api");

                  });

frontend.WithReference(yarp);
builder.Build().Run();
