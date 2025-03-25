using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components.Dialog;

//specify attribute to only allow on BaseAction class
[AttributeUsage(validOn: AttributeTargets.Property)]
public sealed class UIFormViewAction : UIField
{
    public int Order { get; set; }
    public Type? CustomComponent { get; init; }
}