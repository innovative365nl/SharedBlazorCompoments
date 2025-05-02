using Microsoft.AspNetCore.Components;

namespace ExampleApp.Extensions;

public partial class CustomBooleanStyle : ComponentBase
{
    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public string NoColor { get; set; } = "red";

    [Parameter]
    public string YesColor { get; set; } = "green";
}
