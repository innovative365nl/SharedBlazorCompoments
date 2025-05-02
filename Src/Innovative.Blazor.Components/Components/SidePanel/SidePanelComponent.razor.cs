using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Innovative.Blazor.Components.Components;

public partial class SidePanelComponent<TModel>(ISidepanelService sidePanelService) : ComponentBase
{
    private IFormComponent? formComponent;

    private bool isCustomDialog;

    [Parameter] public bool IsEditing { get; set; }

    [Parameter] public bool ShowClose { get; set; } = true;
    [Parameter] public bool ShowEdit { get; set; } = true;
    [Parameter] public bool ShowDelete { get; set; } = false;

    [Parameter] public SideDialogOptions? Options { get; set; }
    [Parameter] public RenderFragment? TitleBarContent { get; set; }
    [Parameter] public RenderFragment? BottomBarContent { get; set; }
    [Parameter] public RenderFragment? ViewChildContent { get; set; }
    [Parameter] public RenderFragment? ActionChildContent { get; set; }
    [Parameter] public RenderFragment? EditChildContent { get; set; }
    [Parameter] public TModel? Model { get; set; }
    [Parameter] public string? Title { get; set; }

    public object? ComponentInstance { get; private set; }

    public void SetFormComponent(IFormComponent? component)
    {
        formComponent = component;

        if (component is object instance)
        {
            ComponentInstance = instance;
        }
    }

    public void OpenCustomDialog()
    {
        isCustomDialog = true;
        StateHasChanged();
    }
    public void CloseCustomDialog()
    {
        isCustomDialog = false;
        StateHasChanged();
    }

    private async Task HandleSaveClick()
    {
        if (formComponent is not null)
        {
            await formComponent
                  .OnFormSubmit()
                  .ConfigureAwait(false);
        }

        if (Model is not null
         && Model.GetType().BaseType == typeof(FormModel))
        {
            (Model as FormModel)?.SaveFormAction?.Invoke();
        }
        
        isCustomDialog = false;
        IsEditing = false;
    }

    private Task HandleDeleteClick()
    {
        if (Model is not null
         && Model.GetType().BaseType == typeof(FormModel))
        {
            (Model as FormModel)?.DeleteFormAction?.Invoke();
        }

        isCustomDialog = false;
        IsEditing = false;
        sidePanelService.CloseSidepanel();
        return Task.CompletedTask;
    }

    private Task HandleCancelClick()
    {
        if (Model is not null
         && Model.GetType().BaseType == typeof(FormModel))
        {
            (Model as FormModel)?.CancelFormAction?.Invoke();
        }

        IsEditing = false;
        isCustomDialog = false;
        ActionChildContent = null;

        return Task.CompletedTask;
    }
}
