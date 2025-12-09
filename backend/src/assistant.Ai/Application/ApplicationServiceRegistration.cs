using Microsoft.Extensions.DependencyInjection;
using Assistant.Ai.Application;
using Assistant.Ai.Application.Services;

namespace Assistant.Ai.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAService>();
        services.AddSingleton<IAHistoryService>();
        return services;
    }
}
