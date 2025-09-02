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
    private string? modelError;

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
    [Parameter] public bool IsNewModel { get; set; } = false;

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

    protected override void OnParametersSet()
    {
        modelError = Model is not null && Model is not FormModel
                         ? $"Cannot render model of type '{Model.GetType().Name}'. Only {nameof(FormModel)} types are supported."
                         : null;
    }

    private async Task HandleSaveClick()
    {
        if (modelError != null)
            return;

        if (Model is FormModel model)
        {
            if (model.Exceptions.Any())
                return;

            if (formComponent is not null)
                await formComponent
                      .OnFormSubmit()
                      .ConfigureAwait(continueOnCapturedContext: true);

            try
            {
                if (model.SaveFormAction is not null)
                {
                    await model.SaveFormAction.Invoke()
                               .ConfigureAwait(continueOnCapturedContext: true);
                    IsNewModel = false;
                }
                if (CloseOnSaveForm)
                {
                    sidePanelService.CloseSidepanel();
                }
                isCustomDialog = false;
                IsEditing = false;
            }
            catch (Exception e)
            {
                await model.AddExceptionAsync(exception: e)
                           .ConfigureAwait(continueOnCapturedContext: false);
            }
            StateHasChanged();
        }
    }

    private async Task HandleDeleteClick()
    {
        if (Model is FormModel model)
        {
            try
            {
                if (model.DeleteFormAction is not null)
                    await model.DeleteFormAction.Invoke()
                               .ConfigureAwait(continueOnCapturedContext: true);

                isCustomDialog = false;
                IsEditing = false;
                sidePanelService.CloseSidepanel();
            }
            catch (Exception e)
            {
                await model.AddExceptionAsync(exception: e)
                           .ConfigureAwait(continueOnCapturedContext: false);
            }
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
                    await model.CancelFormAction.Invoke()
                               .ConfigureAwait(continueOnCapturedContext: true);

                model.ClearExceptions();

                IsEditing = false;
                isCustomDialog = false;
                ActionChildContent = null;
            }
            catch (Exception e)
            {
                await model.AddExceptionAsync(exception: e)
                           .ConfigureAwait(continueOnCapturedContext: false);
            }
        }
    }
}
