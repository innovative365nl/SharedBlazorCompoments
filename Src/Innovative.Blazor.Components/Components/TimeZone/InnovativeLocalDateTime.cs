using System.Diagnostics;
using System.Globalization;
using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Innovative.Blazor.Components.Components;

public sealed class InnovativeLocalDateTime  : ComponentBase, IDisposable
{
    [Inject]
    public required ILocalTimeProvider TimeProvider { get; set; }

    [Parameter]
    public DateTime? DateTime { get; set; }

    [Parameter]
    public DateOnly? DateOnly { get; set; }

    [Parameter]
    public string Format { get; set; } = "dd-MM-yyyy HH:mm:ss";

    public void Dispose()
    {
        TimeProvider.LocalTimeZoneChanged -= LocalTimeZoneChanged;
    }

    protected override void OnInitialized()
    {
        TimeProvider.LocalTimeZoneChanged += LocalTimeZoneChanged;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (DateTime.HasValue)
        {
            var localDateTime = TimeProvider.ToLocalDateTime(DateTime.Value);
            Debug.Assert(builder != null, nameof(builder) + " != null");
            builder.AddContent(0, localDateTime.ToString(Format, CultureInfo.InvariantCulture));
        }
        if (DateOnly.HasValue)
        {
            var dateFormat = new string(Format.TakeWhile(c => !char.IsWhiteSpace(c) && c != 'H' && c != 'h' && c != 'm' && c != 's' && c != 't').ToArray());
            var localDateTime = TimeProvider.ToLocalDateTime(DateOnly.Value);
            Debug.Assert(builder != null, nameof(builder) + " != null");
            builder.AddContent(0, localDateTime.ToString(dateFormat, CultureInfo.InvariantCulture));
        }
    }

    private void LocalTimeZoneChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }
}
