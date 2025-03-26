using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Innovative.Blazor.Components.Components.Dialog;

public partial class RightSideDialog<TModel>(ICustomDialogService dialogService) : ComponentBase
{
    private IDynamicBaseComponent? _formComponent;
    private bool _isCustomDialog;

    private bool _isEditing;

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

    public void SetFormComponent(IDynamicBaseComponent formComponent)
    {
        _formComponent = formComponent;
        // Store the component instance
        if (formComponent is object instance)
        {
            ComponentInstance = instance;
        }
    }

    public void SetCustomDialog(bool isCustom)
    {
        _isCustomDialog = isCustom;
        StateHasChanged();
    }

    private Task HandleCloseClick()
    {
     //   dialogService.CloseSide();
        return Task.CompletedTask;
    }

    private async Task HandleSaveClick()
    {
        if (_formComponent != null) await _formComponent.OnSubmitPressed().ConfigureAwait(false);
        _isCustomDialog = false;
        _isEditing = false;


        await SaveClicked.InvokeAsync().ConfigureAwait(false);
    }
    private Task HandleCancelClick()
    {
        _isEditing = false;
        _isCustomDialog = false;
        ActionChildContent = null;
        return Task.CompletedTask;
    }
}