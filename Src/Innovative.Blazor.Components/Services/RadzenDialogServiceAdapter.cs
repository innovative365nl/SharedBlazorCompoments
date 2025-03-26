#region

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Radzen;

#endregion

namespace Innovative.Blazor.Components.Services;

public interface ICustomDialogService
{
    Task<dynamic> OpenSideAsync<T>(string title, Dictionary<string, object> parameters, SideDialogOptions options)
        where T : ComponentBase;

    void CloseSide();
    void Dispose();
}

[ExcludeFromCodeCoverage]
internal class RadzenDialogServiceAdapter(DialogService dialogService) : ICustomDialogService
{
    public Task<dynamic> OpenSideAsync<T>(string title, Dictionary<string, object> parameters,
        SideDialogOptions options) where T : ComponentBase
    {
        return dialogService.OpenSideAsync<T>(title, parameters, options);
    }

    public void CloseSide()
    {
        dialogService.CloseSide();
    }

    public void Dispose()
    {
        dialogService.Dispose();
    }
}