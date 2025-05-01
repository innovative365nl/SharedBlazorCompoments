using ExampleApp.Pages;
using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Components;

public partial class PasswordUpdateComponent : ComponentBase, IFormComponent
{
    private static int counter = 1;

    [Parameter] public object? Model { get; set; }

    [Parameter] public SidePanelComponent<PersonDisplayModel>? ParentDialog { get; set; }

    // if a parameter with name "ActionProperty" does not exist
    // the component crashes on render
    [Parameter] public string? ActionProperty { get; set; }

    protected override void OnParametersSet()
    {
        if (ParentDialog is not null)
        {
            ParentDialog.SetFormComponent(component: this);
        }
    }

    public Task OnFormSubmit()
    {
        if (ParentDialog is not null && Model is PersonDisplayModel person)
        {
            person.UpdatePasswordAction!.Invoke(obj: ++counter);
        }
        // After click, close the form.
        return OnFormReset(); 
    }

    public Task OnFormReset()
    {
        if (ParentDialog is not null)
        {
            ParentDialog.CloseCustomDialog();
        }
        return Task.CompletedTask;
    }
}
