using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Innovative.Blazor.Components.Components;

public sealed partial class SidepanelHost(ISidepanelService sidepanelService) : ComponentBase, IDisposable
{
    protected override void OnInitialized()
    {
        sidepanelService.OnStateChanged += StateHasChanged;
    }

    public void Dispose()
    {
        sidepanelService.OnStateChanged -= StateHasChanged;
    }

    private void Close()
    {
        sidepanelService.CloseSidepanel();
    }

    private void OnOverlayClick(MouseEventArgs e)
    {
        sidepanelService.CloseSidepanel();
    }
}
