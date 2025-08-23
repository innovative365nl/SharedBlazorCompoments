#region

using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Localizer;
using Innovative.Blazor.Components.Services;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

#endregion

namespace Innovative.Blazor.Components.Common.Composer;

public static class RegisterDependencies
{
    public static IServiceCollection RegisterInnovativeComponents(this IServiceCollection services, string? dateTimeFormat = "dd-MM-yyyy HH:mm:ss")
    {
        services.AddLocalization();
        services.AddRadzenComponents();

        services.AddScoped<IInnovativeSidePanelService, InnovativeSidePanelService>();
        services.AddScoped<ISidepanelService, SidepanelService>();
        services.AddScoped<ILocalTimeProvider>(provider =>
        {
            var localTimeProvider = string.IsNullOrEmpty(dateTimeFormat)
                                        ? new LocalTimeProvider()
                                        : new LocalTimeProvider(dateTimeFormat);
            return localTimeProvider;
        });
        return services;
    }

    public static IServiceCollection AddCustomLocalizer<TFallback>(this IServiceCollection services)
    {
        services.AddScoped<IInnovativeStringLocalizerFactory, InnovativeStringLocalizerFactory>();

        services.AddScoped(serviceType: typeof(IInnovativeStringLocalizer<>),
            implementationType: typeof(InnovativeStringLocalizer<>));
        services.AddSingleton(implementationFactory: provider => typeof(TFallback));

        return services;
    }
}
