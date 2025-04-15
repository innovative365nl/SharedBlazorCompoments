#region

using Innovative.Blazor.Components.Components.Common;
using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

#endregion

namespace Innovative.Blazor.Components.Components.SidePanel;

public partial class SidePanelComponent<TModel>(ICustomDialogService sidePanelService) : ComponentBase
{
    private IDynamicBaseComponent? formComponent;
    private bool isCustomDialog;

    [Parameter] public bool IsEditing { get; set; } 

    [Parameter] public bool ShowClose { get; set; } = true;
    [Parameter] public bool ShowEdit { get; set; } = true;

    [Parameter] public EventCallback SaveClicked { get; set; }
    [Parameter] public SideDialogOptions? Options { get; set; }
    [Parameter] public RenderFragment? TitleBarContent { get; set; }
    [Parameter] public RenderFragment? BottomBarContent { get; set; }
    [Parameter] public RenderFragment? ViewChildContent { get; set; }

    [Parameter] public RenderFragment? ActionChildContent { get; set; }
    [Parameter] public RenderFragment? EditChildContent { get; set; }
    [Parameter] public TModel? Model { get; set; }

    [Parameter] public string? Title { get; set; }
    public object? ComponentInstance { get; private set; }

    public void SetFormComponent(IDynamicBaseComponent component)
    {
        formComponent = component;
        // Store the component instance
        if (component is object instance)
        {
            ComponentInstance = instance;
        }
    }

    public void SetCustomDialog(bool isCustom)
    {
        isCustomDialog = isCustom;
        StateHasChanged();
    }

    private async Task HandleSaveClick()
    {
        if (formComponent != null) await formComponent.OnSubmitPressed().ConfigureAwait(false);
        isCustomDialog = false;
        IsEditing = false;

        await SaveClicked.InvokeAsync().ConfigureAwait(false);
    }

    private Task HandleCancelClick()
    {
        IsEditing = false;
        isCustomDialog = false;
        ActionChildContent = null;
        return Task.CompletedTask;
    }
}
