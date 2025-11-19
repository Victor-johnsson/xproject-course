using System.Text.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using XProjectIntegrationsBackend.Controllers;
using XProjectIntegrationsBackend.Interfaces;
using XProjectIntegrationsBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.AddRedisClient("redis");

// builder.AddKeyedRedisClient("redis");
builder.AddAzureServiceBusClient("serviceBus");
builder.AddAzureBlobServiceClient("blobs");

builder.Services.AddSingleton<ServiceBusPublisherService>();
builder.Services.AddSingleton<EventGridService>();
builder.Services.AddSingleton<IImageService, ImageService>();

builder.Services.AddHttpClient<IPaymentsService, PaymentsService>(static client =>
    client.BaseAddress = new("https+http://paymentsService")
);
builder.Services.AddHttpClient<IPimService, PimService>(static client =>
    client.BaseAddress = new("https+http://productCatalog")
);

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder
    .Services.AddAuthorizationBuilder()
    .AddPolicy(
        "MustBeAdmin",
        policy =>
        {
            policy.RequireRole("Admin.Write");
            policy.RequireAuthenticatedUser();
        }
    );

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

// Configure HTTP request pipeline
app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
