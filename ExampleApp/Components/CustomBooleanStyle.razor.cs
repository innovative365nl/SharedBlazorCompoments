using Microsoft.AspNetCore.Components;

namespace ExampleApp.Components;

public partial class CustomBooleanStyle : ComponentBase
{
    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public string NoColor { get; set; } = "red";

    [Parameter]
    public string YesColor { get; set; } = "green";

    [Parameter]
    public EventCallback ValueChanged { get; set; }
}
