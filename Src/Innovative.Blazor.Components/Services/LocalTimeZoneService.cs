using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Innovative.Blazor.Components.Services;

[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
public class LocalTimeZoneService :ComponentBase
{
    [Inject] public ILocalTimeProvider LocalProvider { get; set; } = default!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    protected override void OnInitialized()
    {
        LocalProvider.LocalTimeZoneChanged += OnLocalTimeZoneChanged;
    }

    private void OnLocalTimeZoneChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        LocalProvider.LocalTimeZoneChanged -= OnLocalTimeZoneChanged;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !LocalProvider.IsLocalTimeZoneSet)
        {
            try
            {
                // Inject getBrowserTimeZone if not present
                await JsRuntime.InvokeVoidAsync("eval", @"
                    if (typeof getBrowserTimeZone !== 'function') {
                        window.getBrowserTimeZone = function() {
                            return Intl.DateTimeFormat().resolvedOptions().timeZone;
                        };
                    }
                ");
                var timeZone = await JsRuntime.InvokeAsync<string>("getBrowserTimeZone");
                LocalProvider.SetLocalTimeZone(timeZone);
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}
