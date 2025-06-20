using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Components;

public partial class CustomBooleanStyle : CustomComponent<bool>
{
    [Parameter]
    public string NoColor { get; set; } = "red";

    [Parameter]
    public string YesColor { get; set; } = "green";
}
