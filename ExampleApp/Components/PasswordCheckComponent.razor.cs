using ExampleApp.Pages;
using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Components;

public partial class PasswordCheckComponent : ComponentBase, IFormComponent
{
    [Parameter] public object? Model { get; set; }

    [Parameter] public SidePanelComponent<PersonModel>? ParentDialog { get; set; }

    // if a parameter with name "ActionProperty" does not exist
    // the component crashes on render
    [Parameter] public string? ActionProperty { get; set; }

    private string? password { get; set; }

    protected override void OnParametersSet()
    {
        if (ParentDialog is not null)
        {
            ParentDialog.SetFormComponent(component: this);
        }
    }

    public Task OnFormSubmit()
    {
        if (ParentDialog is not null && Model is PersonModel person)
        {
            var result = !string.IsNullOrEmpty(password) &&
                            password.Any(x=> char.IsDigit(x)) &&
                            password.Any(x => char.IsUpper(x)) &&
                            password.Any(x => char.IsLower(x));

            person.PasswordCheckAction!.Invoke(obj: result);
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
