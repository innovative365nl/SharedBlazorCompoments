using System.Reflection;
using Innovative.Blazor.Components.Attributes;
using Innovative.Blazor.Components.Components.Dialog;
using Innovative.Blazor.Components.Enumerators;
using Innovative.Blazor.Components.Localizer;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Innovative.Blazor.Components.Services;

public interface IInnovativeDialogService
{
    Task<T> OpenDynamicFormDialog<T>(T model) where T : class;
    void Dispose();
}

internal sealed class InnovativeDialogService(ICustomDialogService dialogService, IInnovativeStringLocalizerFactory localizerFactory): IDisposable, IInnovativeDialogService
{

    public async Task<T> OpenDynamicFormDialog<T>(T model) where T : class
    {
        RightSideDialog<T>? dialogRef = null;

        var viewContent = new RenderFragment(builder =>
        {
            builder.OpenComponent<DynamicDisplayView<T>>(sequence: 0);
            builder.AddAttribute(sequence: 1, name: "Model", value: model);
            builder.CloseComponent();
        });

        var editContent = new RenderFragment(builder =>
        {
            builder.OpenComponent<DynamicFormView<T>>(sequence: 0);
            builder.AddAttribute(sequence: 1, name: "Model", value: model);
            builder.CloseComponent();
        });
        
        var title = GetFormTitle<T>();

        var parameters = new Dictionary<string, object>
        {
            {"Title", title},
            {"Model", model},
            {"ShowEdit", true},
            {"ShowClose", true},
            {"ViewChildContent", viewContent},
            {"EditChildContent", editContent}
        };

        var dialogOptions = new SideDialogOptions
        {
            Width = GetWidth(width: SideDialogWidth.Normal),
            ShowTitle = false,
            ShowMask = false,
            CloseDialogOnOverlayClick = false,
            ShowClose = true
        };

        var result = await dialogService.OpenSideAsync<RightSideDialog<T>>(
            title: title,
            parameters: parameters,
            options: dialogOptions).ConfigureAwait(false);

        dialogRef = result as RightSideDialog<T>;

        return model;
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
            SideDialogWidth.Normal     => "40vw",
            SideDialogWidth.Large      => "60vw",
            SideDialogWidth.ExtraLarge => "80vw",
            _                          => "30vw"
        };
        return $"{size};";
    }

    public void Dispose()
    {
      //  dialogService.Dispose();
    }
}