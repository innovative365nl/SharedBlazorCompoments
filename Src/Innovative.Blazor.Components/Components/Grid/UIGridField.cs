using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components.Grid;

[AttributeUsage(validOn: AttributeTargets.Property)]
public sealed class UIGridField : UIField
{

    public UIGridField(bool showByDefault = true)
    {
        ShowByDefault = showByDefault;
    }
    [ExcludeFromCodeCoverage]
    public UIGridField(Type componentType, bool showByDefault = true)
    {
        CustomComponentType = componentType;
        ShowByDefault = true;
    }
    public bool ShowByDefault { get;  }
    public bool IsSticky { get; set; } 
    public Type? CustomComponentType { get; set; }
    public bool Sortable { get; set; }
    public bool? Filterable { get; set; }
    public string[]? Parameters { get;  set; }
    public Type? ComponentType { get; internal set; }
}