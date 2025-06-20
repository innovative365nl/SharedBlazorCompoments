using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Innovative.Blazor.Components.Components;

[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
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
    [Parameter] public string? DataTestId { get; set; }

    [Parameter] public string? Title { get; set; }

    [Parameter] public bool CloseOnSaveForm { get; set; } = false;

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
        if (ViewChildContent == null && EditChildContent == null)
        {
            return;
        }
        StateHasChanged();
    }
    public void CloseCustomDialog()
    {
        isCustomDialog = false;
        if (ViewChildContent == null && EditChildContent == null)
        {
            return;
        }
        StateHasChanged();
    }

    private async Task HandleSaveClick()
    {
        if (formComponent is not null)
        {
            await formComponent
                  .OnFormSubmit()
                  .ConfigureAwait(true);
        }

        if (Model is FormModel model)
        {
            try
            {
                if (model.SaveFormAction is not null)
                {
                    await model.SaveFormAction!.Invoke().ConfigureAwait(true);

                }
                if (CloseOnSaveForm)
                {
                    isCustomDialog = false;
                    IsEditing = false;
                    sidePanelService.CloseSidepanel();
                }
            }
            catch (Exception e)
            {
                await model.AddExceptionAsync(e).ConfigureAwait(false);
            }
        }
        else
        {
            if (CloseOnSaveForm)
            {
                isCustomDialog = false;
                IsEditing = false;
                sidePanelService.CloseSidepanel();
            }
        }


    }

    private async Task HandleDeleteClick()
    {

        if (Model is FormModel model)
        {
            try
            {
                if(model.DeleteFormAction is not null)
                 await (model.DeleteFormAction.Invoke()!).ConfigureAwait(true);
               isCustomDialog = false;
               IsEditing = false;
               sidePanelService.CloseSidepanel();

            }
            catch (Exception e)
            {
                await model.AddExceptionAsync(e).ConfigureAwait(false);
            }
        }
        else
        {
            isCustomDialog = false;
            IsEditing = false;
            sidePanelService.CloseSidepanel();
        }
    }

    private async Task HandleCancelClick()
    {
        if (formComponent is not null)
        {
            await formComponent
                  .OnFormReset()
                  .ConfigureAwait(true);
        }
        if (Model is FormModel model)
        {
            try
            {
                if(model.CancelFormAction is not null)
                    await (model.CancelFormAction!.Invoke()!).ConfigureAwait(true);
                IsEditing = false;
                isCustomDialog = false;
                ActionChildContent = null;
            }
            catch (Exception e)
            {
                await model.AddExceptionAsync(e).ConfigureAwait(false);
            }
            model.CancelFormAction?.Invoke();
        }
        else
        {
            IsEditing = false;
            isCustomDialog = false;
            ActionChildContent = null;
        }



    }
}
