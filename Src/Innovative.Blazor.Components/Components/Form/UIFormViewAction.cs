using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components;

[ExcludeFromCodeCoverage]
[AttributeUsage(validOn: AttributeTargets.Property)]
public sealed class UIFormViewAction(string name) : UIField(name)
{
    public int Order { get; set; }
    public Type? CustomComponent { get; init; }

    // Conditional visibility support
    // When set, the action is only visible if the specified property on the model meets the condition.
    // - If VisibleWhenEquals is null: the property must be a boolean and equal to true.
    // - If VisibleWhenEquals is set: the property's value (converted to string) must equal this value (case-insensitive).
    //   Works for string, enum, numeric types (comparison uses ToString of the property value).
    // You can invert the result by setting InvertCondition=true.
    public string? VisibleWhenProperty { get; init; }
    public string? VisibleWhenEquals { get; init; }
    public bool InvertCondition { get; init; }
}
