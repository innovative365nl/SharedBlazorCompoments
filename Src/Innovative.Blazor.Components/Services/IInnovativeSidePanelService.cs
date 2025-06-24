using System.Reflection;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Enumerators;
using Innovative.Blazor.Components.Localizer;
using Microsoft.AspNetCore.Components;

namespace Innovative.Blazor.Components.Services;

public interface IInnovativeSidePanelService
{
    /// <summary>
    /// Returns true if the side panel is visible, otherwise false.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Opens a side panel dialog with the specified model in display mode.
    /// </summary>
    Task OpenInDisplayMode<T>(T model, bool showEdit = true, bool showClose = true, bool showDelete = false, string? dataTestId = null) where T : class;

    /// <summary>
    /// Opens a side panel dialog with the specified model in edit mode as a new instance of <code>T</code> is created.
    /// </summary>
    Task OpenInEditMode<T>(T model, bool showClose = true, bool showDelete = false, string? dataTestId = null, bool closeOnSaveForm = false, bool isNewModel = false) where T : class;
    
    /// <summary>
    /// Closes the side panel dialog if it is open.
    /// </summary>
    void ClosePanel<T>(T model) where T : class;
}

internal sealed class InnovativeSidePanelService
(
    ISidepanelService sidePanelService,
    IInnovativeStringLocalizerFactory localizerFactory
) : IInnovativeSidePanelService
{
    public bool IsVisible => sidePanelService.IsVisible;

    public async Task OpenInEditMode<T>(T model, bool showClose = true, bool showDelete = false, string? dataTestId = null, bool closeOnSaveForm = false, bool isNewModel = true) where T : class
    {
        await OpenDynamicFormDialogWithOptions(model: model,  isEditing: true, showEdit: true, showClose: showClose, showDelete: showDelete, dataTestId, closeOnSaveForm, isNewModel)
                .ConfigureAwait(false);
    }

    public async Task OpenInDisplayMode<T>(T model, bool showEdit = true, bool showClose = true, bool showDelete = false,  string? dataTestId = null) where T : class
    {
        await OpenDynamicFormDialogWithOptions(model: model,  isEditing: false, showEdit: showEdit, showClose: showClose, showDelete: showDelete, dataTestId)
                .ConfigureAwait(false);
    }

    private async Task OpenDynamicFormDialogWithOptions<T>
    ( T model
    , bool isEditing = false
    , bool showEdit = true
    , bool showClose = true
    , bool showDelete = false
    , string? dataTestId = null
    , bool closeOnSaveForm = false
    , bool isNewModel = false
    ) where T : class
    {
        var viewContent = new RenderFragment(builder =>
        {
            builder.OpenComponent<InnovativeDetail<T>>(sequence: 0);
            builder.AddAttribute(sequence: 1, name: "Model", value: model);
            builder.CloseComponent();
        });

        var editContent = new RenderFragment(builder =>
        {
            builder.OpenComponent<InnovativeForm<T>>(sequence: 0);
            builder.AddAttribute(sequence: 1, name: "Model", value: model);
            builder.CloseComponent();
        });

        var title = GetFormTitle<T>();

        var parameters = new Dictionary<string, object>
        {
            { "Title", title },
            { "Model", model },
            { "ShowEdit", showEdit },
            { "ShowClose", showClose },
            { "ShowDelete", showDelete },
            { "ViewChildContent", viewContent },
            { "EditChildContent", editContent },
            { "IsEditing", isEditing},
            {"DataTestId", dataTestId ?? string.Empty },
            {"CloseOnSaveForm", closeOnSaveForm},
            {"IsNewModel", isNewModel}
        };

        var options = new SidepanelOptions
        {
            Title = title,
            Width = GetWidth(width: SideDialogWidth.Normal)
        };

        await sidePanelService
              .OpenSidepanelAsync<SidePanelComponent<T>>(parameters, options)
              .ConfigureAwait(false);
    }

    private string GetFormTitle<T>() where T : class
    {
        var type = typeof(T);
        var formAttribute = type.GetCustomAttribute<UIFormClass>();

        if (formAttribute != null && !string.IsNullOrEmpty(formAttribute.Title))
        {
            var resourceType = formAttribute.ResourceType ?? typeof(T);

            var localizer = localizerFactory.Create(resourceType);

            return localizer[formAttribute.Title];
        }

        return type.Name;
    }

    private static string GetWidth(SideDialogWidth width)
    {
        var size = width switch
        {
            SideDialogWidth.Normal => "40vw",
            SideDialogWidth.Large => "60vw",
            SideDialogWidth.ExtraLarge => "80vw",
            _ => "30vw"
        };
        return size;
    }

    public void ClosePanel<T>(T model) where T : class
    {
        if (IsVisible)
        {
            sidePanelService.CloseSidepanel(model);
        }
    }
}
