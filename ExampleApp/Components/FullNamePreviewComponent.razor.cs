using ExampleApp.Pages;
using Innovative.Blazor.Components.Components;

namespace ExampleApp.Components;

public partial class FullNamePreviewComponent : CustomComponent<PersonPreviewModel>
{
    public new PersonPreviewModel? Value
    {
        get => base.Value;
        set
        {
            if (base.Value != value)
            {
                base.Value = value;
                ValueChanged.InvokeAsync(arg: value);
                StateHasChanged();
            }
        }
    }
}
