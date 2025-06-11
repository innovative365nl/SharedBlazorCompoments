using Microsoft.AspNetCore.Components;

namespace Innovative.Blazor.Components.Components;

public abstract class CustomComponent<T>: ComponentBase
{
    [Parameter]
    public T? Value { get; set; }

    [Parameter]
    public string DataTestId { get; set; } = string.Empty;

    [Parameter]
    public EventCallback ValueChanged { get; set; }

    protected void OnValueChanged()
    {
        if (ValueChanged.HasDelegate)
        {
            ValueChanged.InvokeAsync(Value);
        }
    }
}
