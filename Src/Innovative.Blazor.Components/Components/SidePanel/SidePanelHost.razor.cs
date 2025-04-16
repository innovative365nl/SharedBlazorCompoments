using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components.Web;

namespace Innovative.Blazor.Components.Components;

public sealed partial class SidePanelHost(ISidepanelService sidePanelService)
{
    protected override void OnInitialized()
    {
        sidePanelService.OnStateChanged += StateHasChanged;
    }

    public void Dispose()
    {
        sidePanelService.OnStateChanged -= StateHasChanged;
    }

    private void Close()
    {
        sidePanelService.CloseSidepanel();
    }

    private void OnOverlayClick(MouseEventArgs e)
    {
        sidePanelService.CloseSidepanel();
    }
}
