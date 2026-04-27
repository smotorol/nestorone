using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using OrderInventory.Api.Health;
using OrderInventory.Api.Middlewares;
using OrderInventory.Application.Abstractions;
using OrderInventory.Application.Services;
using OrderInventory.Infrastructure.Options;
using OrderInventory.Infrastructure.Persistence;
using OrderInventory.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

var oracleConnectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("OracleDb")
    ?? throw new InvalidOperationException("Oracle connection string is not configured.");

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath |
                            Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod |
                            Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode;
});

builder.Services.AddSingleton(new OracleDbOptions { ConnectionString = oracleConnectionString });

builder.Services.AddSingleton<IOracleConnectionFactory, OracleConnectionFactory>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddHealthChecks()
    .AddCheck("oracle", new OracleConnectionHealthCheck(oracleConnectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Order Inventory API", Version = "v1" });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseHttpLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description
            }),
            timestampUtc = DateTimeOffset.UtcNow
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
});
app.MapControllers();

app.Run();
