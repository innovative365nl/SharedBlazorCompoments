using Microsoft.AspNetCore.Components;

namespace Innovative.Blazor.Components.Components;

public abstract class CustomComponent<T>: ComponentBase
{
    [Parameter]
    public T? Value { get; set; }

    [Parameter]
    public EventCallback ValueChanged { get; set; }

    protected void OnValueChanged() => ValueChanged.InvokeAsync(Value);
}
