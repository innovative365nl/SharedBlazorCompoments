using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components.Grid;

[AttributeUsage(validOn: AttributeTargets.Class)]
public class UIGridClass : UIClass
{
    public bool AllowSorting { get; set; } = true;
    public string? DefaultSortField { get; set; }
}