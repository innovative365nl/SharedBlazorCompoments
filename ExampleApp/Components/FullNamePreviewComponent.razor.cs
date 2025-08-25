using ExampleApp.Pages;
using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Components;

public partial class FullNamePreviewComponent : CustomComponent<Person4Model>
{
    [Parameter]
    public bool DisplayLabel { get; set; }

    public new Person4Model? Value
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
