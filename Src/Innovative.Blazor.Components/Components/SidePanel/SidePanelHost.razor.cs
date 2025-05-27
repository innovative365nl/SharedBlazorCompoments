using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components.Web;

namespace Innovative.Blazor.Components.Components;

public sealed partial class SidePanelHost(ISidepanelService sidePanelService)
{
    protected override void OnInitialized()
    {
        sidePanelService.OnStateChanged += StateHasChanged;
        sidePanelService.VisibleChanged += VisibleChanged;
    }
    private bool IsVisable = true;
    private async void VisibleChanged(bool obj)
    {
        if(obj == IsVisable)
        {}
        else
        {
            if (obj == false)
            {
                await Task.Delay(500).ConfigureAwait(true);
            }
            IsVisable = obj;
            StateHasChanged();
        }
    }

    public void Dispose() => sidePanelService.OnStateChanged -= StateHasChanged;

    private void Close() => sidePanelService.CloseSidepanel();

    private void OnOverlayClick(MouseEventArgs e) => sidePanelService.CloseSidepanel();
}
