using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components;

[ExcludeFromCodeCoverage]
[AttributeUsage(validOn: AttributeTargets.Property)]
public sealed class UIFormViewAction(string name) : UIField(name)
{
    public int Order { get; set; }
    public Type? CustomComponent { get; init; }
}
