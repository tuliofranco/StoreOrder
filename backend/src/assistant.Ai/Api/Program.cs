using System.Net.Http.Json;
using DotNetEnv;
using Assistant.Ai.Api.DTOs;
using Microsoft.AspNetCore.Http.Json;
using Assistant.Ai.Application;
using OpenAI;
using Assistant.Ai.Application.Services;
using Assistant.Ai.Application.Entities;
using Assistant.Ai.Api.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(sp =>
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                ?? throw new Exception("OPENAI_API_KEY not found.");

    return new OpenAIClient(apiKey);
});

builder.Services.AddHttpClient("order-api", client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ORDER_API_URL")
        ?? throw new Exception("ORDER_API_URL missing"));
});

builder.Services.AddApplicationServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var conn = Environment.GetEnvironmentVariable("MONGO_CONNECTION")
              ?? throw new Exception("MONGO_CONNECTION missing");
    return new MongoClient(conn);
});

builder.Services
    .AddHealthChecks()
    .AddCheck<AiApiHealthCheck>("assitant-api")
    .AddCheck<MongoDbHealthCheck>("assistant-mongo");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "Order IA API v1");
    o.RoutePrefix = "swagger";
});

app.UseCors("default");

app.MapPost("/ask", async (AskRequest request, IAService iaService, IAHistoryService historyService) =>
{
    var response = await iaService.AnswerAsync(request.Pergunta);
    await historyService.SaveAsync(
        question: request.Pergunta,
        response: response
    );

    return Results.Ok(response);
})
.WithName("AskIa")
.WithOpenApi();

app.MapGet("/history", async (IAHistoryService history) =>
{
    var items = await history.ListAsync();
    return Results.Ok(items);
})
.WithName("IaHistory")
.WithOpenApi();


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
.WithName("HealthSummary")
.WithOpenApi(); 

app.Run();
