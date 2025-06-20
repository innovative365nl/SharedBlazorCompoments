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

    private void OnOverlayClick(MouseEventArgs e) => sidePanelService.CloseSidepanel();
}
