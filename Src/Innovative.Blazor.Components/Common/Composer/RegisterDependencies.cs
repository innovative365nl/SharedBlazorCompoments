#region

using Innovative.Blazor.Components.Localizer;
using Innovative.Blazor.Components.Services;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

#endregion

namespace Innovative.Blazor.Components.Common.Composer;

public static class RegisterDependencies
{
    public static IServiceCollection RegisterInnovativeComponents(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddRadzenComponents();
        services.AddScoped<IInnovativeDialogService, InnovativeDialogService>();
        services.AddScoped<ICustomDialogService, RadzenDialogServiceAdapter>();
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