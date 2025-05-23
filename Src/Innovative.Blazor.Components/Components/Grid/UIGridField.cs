using System.Diagnostics.CodeAnalysis;

namespace Innovative.Blazor.Components.Components;

[AttributeUsage(validOn: AttributeTargets.Property)]
public sealed class UIGridField : Attribute
{
    /// <summary>
    ///     Returns the name of the field, if any.
    /// </summary>
    /// <remarks>
    ///     The name is used as a key to get the resource string. If the resource is not found it uses the name as a caption.
    /// </remarks>
    public string? Name { get; set; }

    public bool IsVisible { get; set; } = true;
    public bool IsSticky { get; set; }
    public Type? CustomComponentType { get; set; }
    public bool IsSortable { get; set; }
    public bool? IsFilterable { get; set; }
    public string[]? Parameters { get; set; }
}
