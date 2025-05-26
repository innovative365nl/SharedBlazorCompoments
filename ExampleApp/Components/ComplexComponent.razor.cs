using Microsoft.AspNetCore.Components;

namespace ExampleApp.Components;

public partial class ComplexComponent : ComponentBase
{
    [Parameter]
    public ComplexModel? Value { get; set; }

    [Parameter]
    public EventCallback ValueChanged { get; set; }
}

