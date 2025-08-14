using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Components;

public partial class ComplexDisplayComponent : CustomComponent<ComplexModel>
{
    // This parameter is used to control the display of the label in the component.
    // The only requirements are: it must be a parameter and its name must be "DisplayLabel".
    // The property type should be as open as possible, so "object?" will do the job.
    [Parameter]
    public object? DisplayLabel { get; set; }
}
