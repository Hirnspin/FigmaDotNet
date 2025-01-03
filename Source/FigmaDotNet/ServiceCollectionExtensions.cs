using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;

namespace FigmaDotNet;

/// <summary>
/// Extension methods for the <see cref="IServiceCollection"/> interface to add a configures Figma http client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a Figma http client to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns><see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddFigmaHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<FigmaHttpClient>(client =>
        {
            client.BaseAddress = new Uri(Constants.FIGMA_API_BASE_URL);
            int timeoutMinutes = configuration.GetValue<int>(Constants.CONFIG_NAME_TIMEOUT_MINUTES, Constants.FALLBACK_VALUE_TIMEOUT_MINUTES);
            client.Timeout = TimeSpan.FromMinutes(timeoutMinutes);
        });

        services.AddSingleton<ILogger<FigmaHttpClient>, Logger<FigmaHttpClient>>();
        services.AddSingleton(configuration);

        return services;
    }
}
