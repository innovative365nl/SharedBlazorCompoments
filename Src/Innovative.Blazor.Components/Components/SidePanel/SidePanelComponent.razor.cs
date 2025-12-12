using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;
using Radzen;

namespace Innovative.Blazor.Components.Components;

[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
public partial class SidePanelComponent<TModel>(ISidepanelService sidePanelService) : ComponentBase, IDisposable
{
    private readonly List<string> progressLog = new();

    private bool _disposed;
    private IFormComponent? formComponent;
    private bool isBusy;
    private bool isCustomDialog;
    private string? modelError;

    [Inject]
    private DialogService DialogService { get; set; } = default!;

    [Parameter]
    public bool IsEditing { get; set; }

    [Parameter]
    public bool ShowClose { get; set; } = true;

    [Parameter]
    public bool ShowEdit { get; set; } = true;

    [Parameter]
    public bool ShowDelete { get; set; } = false;

    [Parameter]
    public SideDialogOptions? Options { get; set; }

    [Parameter]
    public RenderFragment? TitleBarContent { get; set; }

    [Parameter]
    public RenderFragment? BottomBarContent { get; set; }

    [Parameter]
    public RenderFragment? ViewChildContent { get; set; }

    [Parameter]
    public RenderFragment? ActionChildContent { get; set; }

    [Parameter]
    public RenderFragment? EditChildContent { get; set; }

    [Parameter]
    public TModel? Model { get; set; }

    [Parameter]
    public string? DataTestId { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public bool CloseOnSaveForm { get; set; } = false;

    [Parameter]
    public bool IsNewModel { get; set; } = false;

    public object? ComponentInstance { get; private set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

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
        if (ViewChildContent == null
         && EditChildContent == null)
        {
            return;
        }

        StateHasChanged();
    }
    public void CloseCustomDialog()
    {
        isCustomDialog = false;
        if (ViewChildContent == null
         && EditChildContent == null)
        {
            return;
        }
        StateHasChanged();
    }

    public void CloseSidepanel() => sidePanelService.CloseSidepanel();
    public void CloseSidepanel(object? result) => sidePanelService.CloseSidepanel(result);

    protected override void OnInitialized()
    {
        // Assign confirmation delegate for overlay clicks
        sidePanelService.BeforeOverlayCloseAsync = ConfirmLeaveIfNeededAsync;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
        {
            // Clear delegate when component is disposed to avoid dangling references
            if (sidePanelService.BeforeOverlayCloseAsync == ConfirmLeaveIfNeededAsync)
            {
                sidePanelService.BeforeOverlayCloseAsync = null;
            }
        }
        _disposed = true;
    }

    private async Task<bool> ConfirmLeaveIfNeededAsync()
    {
        // Only confirm when user clicks outside (overlay handled by host)
        if (isCustomDialog || IsEditing)
        {
            var isDutch = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("nl", StringComparison.OrdinalIgnoreCase);
            var title = isDutch ? "Bevestigen" : "Confirm";
            var message = isDutch
                              ? "Weet je zeker dat je dit paneel wilt verlaten? Niet-opgeslagen wijzigingen gaan verloren."
                              : "Are you sure you want to leave this panel? Unsaved changes will be lost.";
            var okText = isDutch ? "Ja, verlaten" : "Yes, leave";
            var cancelText = isDutch ? "Nee, annuleren" : "No, cancel";

            var result = await DialogService.Confirm(message
                                                   , title
                                                   , new ConfirmOptions
                                                     {
                                                         OkButtonText = okText
                                                       , CancelButtonText = cancelText
                                                     })
                                            .ConfigureAwait(continueOnCapturedContext: true);

            return result == true;
        }
        return true;
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
            // Allow retry even if there are existing exceptions from previous attempts.
            // Clear previous exceptions so the user sees only the latest outcome.
            model.ClearExceptions();

            if (formComponent is not null)
                await formComponent
                      .OnFormSubmit()
                      .ConfigureAwait(continueOnCapturedContext: true);

            EventHandler<ProgressEventArgs>? progressHandler = null;
            try
            {
                // subscribe to progress
                progressLog.Clear();
                progressHandler = (_, e) =>
                                  {
                                      progressLog.Add(e.Message);
                                      _ = InvokeAsync(StateHasChanged);
                                  };
                model.OnProgress += progressHandler;

                isBusy = true;
                await InvokeAsync(StateHasChanged).ConfigureAwait(false);
                await Task.Yield();

                if (model.SaveFormAction is not null)
                {
                    await model.SaveFormAction
                               .Invoke()
                               .ConfigureAwait(continueOnCapturedContext: false);
                }

                await InvokeAsync(() =>
                                  {
                                      IsNewModel = false;
                                      if (CloseOnSaveForm)
                                      {
                                          sidePanelService.CloseSidepanel();
                                      }

                                      isCustomDialog = false;
                                      IsEditing = false;
                                  })
                    .ConfigureAwait(false);
            }
            catch (ApiException ex)
            {
                await model.AddExceptionAsync(exception: ex).ConfigureAwait(false);
                //  model.AddAlert(AlertSeverity.Error, "Action error", detail: ex.Message, inForm: true, inDetail: true);
            }
            catch (InvalidOperationException ex)
            {
                await model.AddExceptionAsync(exception: ex).ConfigureAwait(false);
                //  model.AddAlert(AlertSeverity.Error, "Action error", detail: ex.Message, inForm: true, inDetail: true);
            }
            finally
            {
                if (progressHandler != null)
                {
                    model.OnProgress -= progressHandler;
                }
                await InvokeAsync(() =>
                                  {
                                      isBusy = false;
                                      StateHasChanged();
                                  })
                    .ConfigureAwait(false);
            }
        }
    }

    private async Task HandleDeleteClick()
    {
        if (Model is FormModel model)
        {
            try
            {
                if (model.DeleteFormAction is not null)
                    await model.DeleteFormAction
                               .Invoke()
                               .ConfigureAwait(continueOnCapturedContext: true);

                isCustomDialog = false;
                IsEditing = false;
                sidePanelService.CloseSidepanel();
            }
            catch (ApiException ex)
            {
                await model.AddExceptionAsync(exception: ex).ConfigureAwait(false);
                model.AddAlert(AlertSeverity.Error, "Action error", detail: ex.Message, inForm: true, inDetail: true);
            }
            catch (InvalidOperationException ex)
            {
                await model.AddExceptionAsync(exception: ex).ConfigureAwait(false);
                model.AddAlert(AlertSeverity.Error, "Action error", detail: ex.Message, inForm: true, inDetail: true);
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
                if (model.CancelFormAction is not null)
                    await model.CancelFormAction
                               .Invoke()
                               .ConfigureAwait(continueOnCapturedContext: true);

                model.ClearExceptions();

                IsEditing = false;
                isCustomDialog = false;
                ActionChildContent = null;
            }
            catch (ApiException ex)
            {
                await model.AddExceptionAsync(exception: ex).ConfigureAwait(false);
                model.AddAlert(AlertSeverity.Error, "Action error", detail: ex.Message, inForm: true, inDetail: true);
            }
            catch (InvalidOperationException ex)
            {
                await model.AddExceptionAsync(exception: ex).ConfigureAwait(false);
                model.AddAlert(AlertSeverity.Error, "Action error", detail: ex.Message, inForm: true, inDetail: true);
            }
        }
    }
}
