using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Infrastructure;
using Order.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Order.Api.Middlewares;
using Order.Api.Controllers.Internal;
using Order.Api.Services;
using Order.Infrastructure.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Order.Infrastructure.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

#pragma warning disable CA1031 // Não capturar exceptions de tipos genéricos
try
{
    if (string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Development",
            StringComparison.OrdinalIgnoreCase))
    {
        DotNetEnv.Env.TraversePath().Load();
    }
}
catch { }
#pragma warning restore CA1031 // Não capturar exceptions de tipos genéricos


var connectionString =
    Environment.GetEnvironmentVariable("STRING_CONNECTION")
    ?? builder.Configuration.GetConnectionString("STRING_CONNECTION");

var connectionStringResdis =
    Environment.GetEnvironmentVariable("REDIS_CONNECTION")
    ?? builder.Configuration.GetConnectionString("REDIS_CONNECTION");

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<ISqlExecutionService, SqlExecutionService>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Order.Core.Application.UseCases.Orders.CreateOrder.CreateOrderCommand).Assembly)
);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(o =>
{
    o.IncludeScopes = true;
});

builder.Logging.AddFilter("", LogLevel.Information); 
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(
        options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(connectionString, connectionStringResdis);


builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Order.Core.Application.UseCases.Orders.CreateOrder.CreateOrderCommand).Assembly)
);

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API alive"))
    .AddCheck<PostgresDbHealthCheck>("postgres")
    .AddCheck<RedisDbHealthCheck>("redis");

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StoreOrderDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
}

app.UseRouting();
app.UseMiddleware<OrderIdRouteScopeMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler();


app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapGet("/health-summary", async (HealthCheckService healthService) =>
{
    var report = await healthService.CheckHealthAsync();

    var result = new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            duration = e.Value.Duration.ToString()
        })
    };

    return Results.Ok(result);
})
.WithName("OrderApiHealthSummary")
.WithTags("Health");

app.Run();
#pragma warning disable CA1515 // Considere tornar internos os tipos públicos
public partial class Program { }
#pragma warning restore CA1515 // Considere tornar internos os tipos públicos
