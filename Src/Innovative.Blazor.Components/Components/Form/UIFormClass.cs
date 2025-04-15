#region

using Innovative.Blazor.Components.Attributes;

#endregion

namespace Innovative.Blazor.Components.Components.Form;

[AttributeUsage(validOn: AttributeTargets.Class, Inherited = false)]
public sealed class UIFormClass(string title) : UIClass
{
    /// <summary>
    ///     a Title for the form as a resource key
    /// </summary>
    public string Title { get; } = title;

    public string[]? ColumnOrder { get; set; }
    public string[]? ColumnWidthNames { get; set; }
    public int[]? ColumnWidthValues { get; set; }
}
