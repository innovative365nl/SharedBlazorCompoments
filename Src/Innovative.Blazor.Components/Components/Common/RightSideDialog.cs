using Microsoft.AspNetCore.Components;
using Innovative.Blazor.Components.Components.SidePanel;
using Innovative.Blazor.Components.Services;

namespace Innovative.Blazor.Components.Components.Common;

/// <summary>
/// Base dialog component for right side panels
/// </summary>
/// <typeparam name="TModel">The model type used by the dialog</typeparam>
public class RightSideDialog<TModel> : SidePanelComponent<TModel>
{
    public RightSideDialog(ICustomDialogService sidePanelService) : base(sidePanelService)
    {
    }

    public string? ActionTitle { get; set; }
}