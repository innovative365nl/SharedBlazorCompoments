using Microsoft.AspNetCore.Components;

namespace Innovative.Blazor.Components.Components.Alert;

public enum AlertSeverity
{
    Info,
    Success,
    Warning,
    Error
}

public partial class InnovativeAlert : ComponentBase
{
    [Parameter] public AlertSeverity Severity { get; set; } = AlertSeverity.Info;
    [Parameter] public string? Summary { get; set; }
    [Parameter] public string? Detail { get; set; }
    [Parameter] public bool Closable { get; set; } = true;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private bool visible = true;

    private void Close()
    {
        visible = false;
        if (OnClose.HasDelegate)
        {
            OnClose.InvokeAsync(null);
        }
    }
}
