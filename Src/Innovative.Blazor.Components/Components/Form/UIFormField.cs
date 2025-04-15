#region

using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Attributes;

#endregion

namespace Innovative.Blazor.Components.Components.Form;

[ExcludeFromCodeCoverage]
[AttributeUsage(validOn: AttributeTargets.Property)]
[SuppressMessage("Design", "CA1019:Define accessors for attribute arguments")]
public sealed class UIFormFieldAttribute : UIField
{
    public UIFormFieldAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
    public string? ColumnGroup { get; set; }
    public bool UseWysiwyg { get; set; }
    public Type? DisplayComponent { get; set; }
    public string[]? DisplayParameters { get; set; }
    public Type? FormComponent { get; set; }
    public string[]? FormParameters { get; set; }
}
