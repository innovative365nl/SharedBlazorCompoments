using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components.Web;

namespace Innovative.Blazor.Components.Components;

public sealed partial class SidePanelHost(ISidepanelService sidePanelService)
{
    private bool IsVisable = true;
    private bool pressStartedOnOverlay;
    protected override void OnInitialized()
    {
        sidePanelService.OnStateChanged += StateHasChanged;
        sidePanelService.VisibleChanged += VisibleChanged;
    }
    private void VisibleChanged(bool obj)
    {
        //todo: check for fix to solve in the dom
        // try
        // {
        //     Console.WriteLine(obj);
        //     if(obj == IsVisable)
        //     {}
        //     else
        //     {
        //         // if (obj == false)
        //         // {
        //         //
        //         //    await Task.Delay(millisecondsDelay: 500).ConfigureAwait(false);
        //         //    if(sidePanelService.IsVisible == false)
        //         //    {
        //         //        IsVisable = false;
        //         //        StateHasChanged();
        //         //
        //         //    }
        //         // }
        //     }
        // }
        // catch (Exception ex)
        // {
        //
        //     Console.WriteLine(ex.Message);
        //     throw; // TODO handle exception
        // }
    }

    public void Dispose()
    {
        sidePanelService.OnStateChanged -= StateHasChanged;
        sidePanelService.VisibleChanged -= VisibleChanged;
    }

    private void Close() => sidePanelService.CloseSidepanel();

    // A click fires on the closest common ancestor of mousedown and mouseup, so dragging a text
    // selection from inside the panel to somewhere outside it lands a click on the overlay.
    // Only a gesture that both started and ended on the overlay counts as clicking outside.
    private void OnOverlayMouseDown(MouseEventArgs e) => pressStartedOnOverlay = true;

    private async Task OnOverlayClick(MouseEventArgs e)
    {
        if (!pressStartedOnOverlay)
        {
            return;
        }

        pressStartedOnOverlay = false;

        if (sidePanelService.BeforeOverlayCloseAsync is null)
        {
            sidePanelService.CloseSidepanel();
            return;
        }
        var canClose = await sidePanelService.BeforeOverlayCloseAsync.Invoke().ConfigureAwait(continueOnCapturedContext: true);
        if (canClose)
        {
            sidePanelService.CloseSidepanel();
        }
    }
}
