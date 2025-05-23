#region

using Innovative.Blazor.Components.Attributes;

#endregion

namespace Innovative.Blazor.Components.Components;

[AttributeUsage(validOn: AttributeTargets.Class)]
public sealed class UIGridClass : UIClass
{
    public bool AllowSorting { get; set; } = true;
    public string? DefaultSortField { get; set; }
}