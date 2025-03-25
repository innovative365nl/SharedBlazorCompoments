using System.Runtime.CompilerServices;
using Microsoft.Extensions.Localization;

[assembly: InternalsVisibleTo("Innovative.Blazor.Components.Tests") ]
namespace Innovative.Blazor.Components.Localizer;

/// <summary>
///     Interface for custom string localizer with primary and fallback behavior
/// </summary>
public interface IInnovativeStringLocalizer : IStringLocalizer
{
    void SetFallbackLocalizer(IStringLocalizer? fallbackLocalizer);
    void SetResourceType(Type resourceType);
}

/// <summary>
///     Base interface for custom string localizer
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IInnovativeStringLocalizer<T> : IInnovativeStringLocalizer
{}
internal class InnovativeStringLocalizer<T> : IInnovativeStringLocalizer<T>
{
    private readonly IStringLocalizerFactory _factory;

    public InnovativeStringLocalizer(IStringLocalizerFactory factory, Type? fallbackResourceType)
    {
        ArgumentNullException.ThrowIfNull(argument: factory);
        _factory = factory;

        PrimaryLocalizer = factory.Create(resourceSource: typeof(T));
        FallbackLocalizer = fallbackResourceType != null ? factory.Create(resourceSource: fallbackResourceType) : null;
    }

    private IStringLocalizer PrimaryLocalizer { get; set; }

    private IStringLocalizer? FallbackLocalizer { get; set; }

    public void SetFallbackLocalizer(IStringLocalizer? fallbackLocalizer)
    {
        FallbackLocalizer = fallbackLocalizer ?? throw new ArgumentNullException(paramName: nameof(fallbackLocalizer));
    }

    public void SetResourceType(Type resourceType)
    {
        if (resourceType == null)
            throw new ArgumentNullException(paramName: nameof(resourceType));

        PrimaryLocalizer = _factory.Create(resourceSource: resourceType);
    }

    public LocalizedString this[string name]
    {
        get
        {
            var result = PrimaryLocalizer[name: name];

            if (result.ResourceNotFound && FallbackLocalizer != null)
            {
                return FallbackLocalizer[name: name];
            }

            return result;
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var result = PrimaryLocalizer[name: name, arguments: arguments];

            if (result.ResourceNotFound && FallbackLocalizer != null)
            {
                return FallbackLocalizer[name: name, arguments: arguments];
            }

            return result;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var primaryStrings = PrimaryLocalizer.GetAllStrings(includeParentCultures: includeParentCultures);

        if (FallbackLocalizer != null)
        {
            var fallbackStrings = FallbackLocalizer.GetAllStrings(includeParentCultures: includeParentCultures);

            var primaryStringKeys = new HashSet<string>();
            var allStrings = new List<LocalizedString>();

            foreach (var str in primaryStrings)
            {
                primaryStringKeys.Add(item: str.Name);
                allStrings.Add(item: str);
            }

            allStrings.AddRange(collection: fallbackStrings.Where(predicate: str => !primaryStringKeys.Contains(item: str.Name)));

            return allStrings;
        }

        return primaryStrings;
    }
}