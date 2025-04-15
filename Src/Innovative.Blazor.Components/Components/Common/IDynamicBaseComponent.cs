namespace Innovative.Blazor.Components.Components.Common;

public interface IDynamicBaseComponent
{
    Task OnSubmitPressed();
    Task OnCancelPressed();
}