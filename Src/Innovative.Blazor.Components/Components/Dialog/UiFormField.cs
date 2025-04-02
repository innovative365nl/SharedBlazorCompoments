#region

using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Attributes;

#endregion

namespace Innovative.Blazor.Components.Components.Dialog;

[ExcludeFromCodeCoverage]
[AttributeUsage(validOn: AttributeTargets.Property)]
[SuppressMessage("Design", "CA1019:Define accessors for attribute arguments")]
public sealed class UIFormFieldAttribute(string name) : UIField
{
    public new required string Name { get; set; } = name;
    public string? ColumnGroup { get; set; }
    public bool UseWysiwyg { get; set; }
    public Type? ViewComponent { get; set; }
    public string[]? ViewParameters { get; set; }
    public Type? FormComponent { get; set; }
    public string[]? FormParameters { get; set; }
}