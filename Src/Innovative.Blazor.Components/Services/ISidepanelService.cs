using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Innovative.Blazor.Components.Enumerators;

namespace Innovative.Blazor.Components.Services;

public class SidepanelOptions
{
    public string? Title { get; set; }
    public string? Width { get; set; } // e.g., "400px", "40%"
    public SideDialogWidth SideDialogWidth { get; set; } = SideDialogWidth.Normal;
    // Add more options as needed
}

public interface ISidepanelService
{
    bool IsVisible { get; }
    Action<bool>? VisibleChanged { get; set; }

    Type? CurrentComponentType { get; }
    Dictionary<string, object>? CurrentParameters { get; }
    SidepanelOptions? CurrentOptions { get; }
    [SuppressMessage("Design", "CA1003:Use generic event handler instances")]
    event Action? OnStateChanged;
    Task<object?> OpenSidepanelAsync<T>(Dictionary<string, object> parameters, SidepanelOptions options) where T : IComponent;
    void CloseSidepanel(object? result = null);
}
