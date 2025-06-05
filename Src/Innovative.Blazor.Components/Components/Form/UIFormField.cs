using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components;

[ExcludeFromCodeCoverage]
[AttributeUsage(validOn: AttributeTargets.Property)]
public sealed class UIFormField(string name) : UIField(name)
{
    public string? ColumnGroup { get; set; }
    public bool UseWysiwyg { get; set; }
    public Type? DisplayComponent { get; set; }
    public string[]? DisplayParameters { get; set; }
    public Type? FormComponent { get; set; }
    public string[]? FormParameters { get; set; }
    public string? TextProperty { get; set; }
    public string DataTestId { get; set; } = string.Empty;
}
