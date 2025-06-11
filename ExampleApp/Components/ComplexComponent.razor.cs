using Innovative.Blazor.Components.Components;

namespace ExampleApp.Components;

public partial class ComplexComponent : CustomComponent<ComplexModel>
{
    protected override void OnParametersSet() => DataTestId = "complex-component";

    public override string ToString() => Value?.Name ?? "Complex Component";
}
