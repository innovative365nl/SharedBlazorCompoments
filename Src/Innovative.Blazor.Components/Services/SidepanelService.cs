using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace Innovative.Blazor.Components.Services;

public class SidepanelService : ISidepanelService
{
    public bool IsVisible { get; private set; }
    public Action<bool>? VisibleChanged { get; set; }
    public Type? CurrentComponentType { get; private set; }
    public Dictionary<string, object>? CurrentParameters { get; private set; }
    public SidepanelOptions? CurrentOptions { get; private set; }
    public event Action? OnStateChanged;

    private TaskCompletionSource<object?>? _tcs;

    public Task<object?> OpenSidepanelAsync<T>(Dictionary<string, object> parameters, SidepanelOptions options) where T : IComponent
    {
        if (IsVisible)
        {
            throw new InvalidOperationException("A sidepanel is already open.");
        }
        IsVisible = true;
        VisibleChanged?.Invoke(IsVisible);
        CurrentComponentType = typeof(T);
        CurrentParameters = parameters;
        CurrentOptions = options;
        _tcs = new TaskCompletionSource<object?>();
        OnStateChanged?.Invoke();
        return _tcs.Task;
    }

    public void CloseSidepanel(object? result = null)
    {

        if (!IsVisible)
            return;
        IsVisible = false;
        VisibleChanged?.Invoke(IsVisible);
        CurrentComponentType = null;
        CurrentParameters = null;
        CurrentOptions = null;
        OnStateChanged?.Invoke();
        _tcs?.TrySetResult(result);
        _tcs = null;
    }
}
