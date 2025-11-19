using Microsoft.EntityFrameworkCore;
using XProject.Integrations.CRM.EntityFramework;
using XProject.Integrations.CRM.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<CrmContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("crm");
    options.UseSqlServer(connectionString, options => options.EnableRetryOnFailure());
});
builder.Services.AddTransient<IOrderService, OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapDefaultEndpoints();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

//app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CrmContext>();

    var pendingMigrations = context.Database.GetPendingMigrations();

    if (pendingMigrations.Any())
    {
        context.Database.Migrate();
    }
}

app.Run();
