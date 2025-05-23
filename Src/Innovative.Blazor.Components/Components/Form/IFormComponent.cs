namespace Innovative.Blazor.Components.Components;

public interface IFormComponent
{
    Task OnFormSubmit();
    Task OnFormReset();
}
