using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components.Dialog;
[ExcludeFromCodeCoverage]
[AttributeUsage(validOn: AttributeTargets.Property)]
public sealed class UIFormFieldAttribute : UIField
{
    public string? ColumnGroup { get; set; }
    public bool UseWysiwyg { get; set; }
    public Type? ViewComponent { get; set; }
    public string[]? ViewParameters { get; set; }
    public Type? FormComponent { get; set; }
    public string[]? FormParameters { get; set; }
}