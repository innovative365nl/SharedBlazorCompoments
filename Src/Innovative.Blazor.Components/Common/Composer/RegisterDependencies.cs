using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Localizer;
using Innovative.Blazor.Components.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Innovative.Blazor.Components.Common.Composer;

[ExcludeFromCodeCoverage]
public static class RegisterDependencies
{
    public static IServiceCollection RegisterInnovativeComponents(this IServiceCollection services)
    {
        services.AddScoped<IInnovativeDialogService, InnovativeDialogService>();
        return services;
    }
    public static IServiceCollection AddCustomLocalizer<TFallback>(this IServiceCollection services)
    {
        services.AddScoped<IInnovativeStringLocalizerFactory, InnovativeStringLocalizerFactory>();

        services.AddScoped(serviceType: typeof(IInnovativeStringLocalizer<>), implementationType: typeof(InnovativeStringLocalizer<>));
        services.AddSingleton(implementationFactory: provider => typeof(TFallback));

        return services;
    }
}