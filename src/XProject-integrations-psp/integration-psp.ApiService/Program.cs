using Dapper;
using Models;
using Npgsql;
using Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.AddAzureNpgsqlDataSource("payments");

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet(
    "/payments",
    async (IPaymentRepository repository) =>
    {
        return await repository.GetAllAsync();
    }
);

app.MapGet(
    "/payments/{id}",
    async (int id, IPaymentRepository repository) =>
    {
        var payment = await repository.GetByIdAsync(id);
        if (payment == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(payment);
    }
);
app.MapPost(
    "/payments",
    async (IPaymentRepository repository) =>
    {
        return Results.Ok(await repository.CreateAsync());
    }
);
app.MapDelete(
    "/payments/{id}",
    async (IPaymentRepository repository, int id) =>
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    }
);
app.MapPut(
    "/payments",
    async (IPaymentRepository repository, Payment payment) =>
    {
        var updated = await repository.UpdateAsync(payment);
        if (!updated)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    }
);

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    NpgsqlDataSource _datasource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
    using var conn = _datasource.CreateConnection();
    await conn.ExecuteAsync(
        @"

            CREATE TABLE IF NOT EXISTS payments (
                id SERIAL PRIMARY KEY,
                paymentcompleted BOOLEAN NOT NULL
            );

        "
    );
}

app.Run();
