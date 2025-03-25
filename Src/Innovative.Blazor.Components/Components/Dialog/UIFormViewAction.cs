using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components.Dialog;

//specify attribute to only allow on BaseAction class
[AttributeUsage(validOn: AttributeTargets.Property)]
public class UIFormViewAction : UIField
{
    public int Order { get; set; } = 0;
    public Type CustomComponent { get; set; }
}