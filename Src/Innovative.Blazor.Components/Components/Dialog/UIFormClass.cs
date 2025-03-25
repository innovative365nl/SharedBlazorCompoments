using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components.Dialog;

[AttributeUsage(validOn: AttributeTargets.Class, Inherited = false)]
public class UIFormClass : UIClass
{
    public UIFormClass(string title)
    {
        Title = title;
    }

    /// <summary>
    /// a Title for the form as a resource key
    /// </summary>
    public string Title { get; set; }

    public string[]? ColumnOrder { get; set; }
    public string[] ColumnWidthNames { get; set; }
    public int[] ColumnWidthValues { get; set; }
}