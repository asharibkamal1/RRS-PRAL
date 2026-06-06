using Microsoft.Extensions.DependencyInjection;

namespace PralPer.Application;

/// <summary>
/// Registers Application-layer services. Feature service *interfaces* live here;
/// data-bound *implementations* are registered by the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Pure application services (validators, mappers, orchestration) register here as they are added.
        return services;
    }
}
