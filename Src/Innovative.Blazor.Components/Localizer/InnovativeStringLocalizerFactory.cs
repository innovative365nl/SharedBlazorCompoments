using Microsoft.Extensions.DependencyInjection;

namespace Innovative.Blazor.Components.Localizer;

public interface IInnovativeStringLocalizerFactory
{
    IInnovativeStringLocalizer Create(Type resourceType);
}

public class InnovativeStringLocalizerFactory(IServiceProvider serviceProvider) : IInnovativeStringLocalizerFactory
{
    public IInnovativeStringLocalizer Create(Type resourceType)
    {
        var localizerType = typeof(InnovativeStringLocalizer<>).MakeGenericType(resourceType);
        
        var localizer = ActivatorUtilities.CreateInstance(serviceProvider, localizerType);
        
        return (IInnovativeStringLocalizer)localizer;
    }
}