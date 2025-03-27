using Microsoft.AspNetCore.Components;

namespace ExampleApp.Extensions;
public partial class CustomBoolStyle : ComponentBase
{
    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public string Color1 { get; set; } = "red";
}