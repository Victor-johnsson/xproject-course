using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.AddCosmosDbContext<PimDbContext>(
    "productsCosmosDb",
    "productsDatabase"
// configureDbContextOptions =>
// {
//     configureDbContextOptions.RequestTimeout = TimeSpan.FromSeconds(120);
// }
);

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();
using var scope = app.Services.CreateScope();
using var context = scope.ServiceProvider.GetRequiredService<PimDbContext>();
await context.Database.EnsureCreatedAsync();

// await context.Products.AddAsync(
//     new Product
//     {
//         Id = "1",
//         Name = "Product 1",
//         Stock = 100,
//         Price = 10,
//     }
// );
await context.SaveChangesAsync();

app.MapGet("/hello", () => "Hello World!");
app.MapPost(
    "/api/products",
    async (CreateProduct product, PimDbContext context, ILogger<Program> logger) =>
    {
        logger.LogInformation("Creating product {product}", product.Name);
        Product newProduct = new()
        {
            Id = Guid.CreateVersion7().ToString(),
            Name = product.Name,
            Stock = product.Stock,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
        };
        _ = await context.AddAsync(newProduct);
        await context.SaveChangesAsync();
        logger.LogInformation("Product with name {product} created", product.Name);
        return newProduct;
    }
);
app.MapGet(
    "/api/products",
    async (PimDbContext context, ILogger<Program> logger) =>
    {
        logger.LogInformation("Getting all products");

        return await context.Products.ToListAsync();
    }
);
app.MapGet(
    "/api/products/{id}",
    async (string id, PimDbContext context, ILogger<Program> logger) =>
    {
        logger.LogInformation("Getting product with ID {id}", id);
        return await context.Products.Where(e => e.Id == id).FirstOrDefaultAsync();
    }
);
app.MapGet(
    "/api/products/updated-last-hour",
    async ([FromServices] PimDbContext dbContext) =>
    {
        var oneHourAgoTimestamp = new DateTimeOffset(
            DateTime.UtcNow.AddHours(-1)
        ).ToUnixTimeSeconds();

        var updatedProducts = await dbContext
            .Products.Where(p => EF.Property<long>(p, "_ts") >= oneHourAgoTimestamp) // Query system property
            .ToListAsync();

        return updatedProducts;
    }
);
app.MapDelete(
    "/api/products/{id}",
    async (string id, PimDbContext context) =>
    {
        Product? product = await context.Products.FirstOrDefaultAsync(e => e.Id == id);
        if (product is not null)
        {
            _ = context.Products.Remove(product);
        }
        _ = await context.SaveChangesAsync();
    }
);

app.MapPut(
    "/api/products/{id}/stock",
    async (string id, int stockCount, PimDbContext context) =>
    {
        Product? product = await context.Products.FirstOrDefaultAsync(e => e.Id == id);

        if (product is null)
        {
            return Results.NotFound($"Product with ID {id} not found.");
        }

        if (product.Stock < stockCount)
        {
            return Results.BadRequest(
                $"Insufficient stock. Available: {product.Stock}, Requested: {stockCount}"
            );
        }

        product.Stock -= stockCount;

        await context.SaveChangesAsync();

        return Results.Ok(product);
    }
);

app.MapDefaultEndpoints();

app.Run();
