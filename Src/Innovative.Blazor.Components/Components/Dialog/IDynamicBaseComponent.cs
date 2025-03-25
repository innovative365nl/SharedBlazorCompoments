namespace Innovative.Blazor.Components.Components.Dialog;

public interface IDynamicBaseComponent
{
    public Task OnSubmitPressed();
    public Task OnCancelPressed();
}