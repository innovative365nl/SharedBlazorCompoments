using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components.Grid;

[AttributeUsage(validOn: AttributeTargets.Property)]
public class UIGridField : UIField
{

    public UIGridField(bool showByDefault = true)
    {
        ShowByDefault = showByDefault;
    }

    public UIGridField(Type componentType, bool showByDefault = true)
    {
        CustomComponentType = componentType;
        ShowByDefault = true;
    }

    public UIGridField(Type componentType, params string[] parameters)
    {
        CustomComponentType = componentType;
        Parameters = parameters;
        ShowByDefault = true;
    }

    public UIGridField(Type componentType, bool showByDefault = true, params string[] parameters)
    {
        CustomComponentType = componentType;
        Parameters = parameters;
        ShowByDefault = true;
    }
    public bool ShowByDefault { get; set; } = true;
    public bool IsSticky { get; set; } = false;
    public Type? CustomComponentType { get; set; }
    public bool Sortable { get; set; }
    public bool? Filterable { get; set; }
    public string[]? Parameters { get; set; }
}