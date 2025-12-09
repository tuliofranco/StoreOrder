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

app.Run();
#pragma warning disable CA1515 // Considere tornar internos os tipos públicos
public partial class Program { }
#pragma warning restore CA1515 // Considere tornar internos os tipos públicos
