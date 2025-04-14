namespace Innovative.Blazor.Components.Components.Form;

public interface IDynamicBaseComponent
{
    public Task OnSubmitPressed();
    public Task OnCancelPressed();
}
