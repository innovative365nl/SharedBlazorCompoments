using ExampleApp.Pages;
using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Extensions;

public partial class PasswordUpdateComponent : ComponentBase
{
    private const int ResultValue = 123;

    private int counter = 6;

    [Parameter] public object? Model { get; set; }

    [Parameter] public SidePanelComponent<PersonDisplayModel>? ParentDialog { get; set; }

    protected override void OnParametersSet()
    {
        if (ParentDialog == null || Model is not PersonDisplayModel person)
        {
            return;
        }

        ParentDialog.SetFormComponent(component: person);
        person.CancelFormAction = () =>
                                  {
                                      ParentDialog?.SetCustomDialog(isCustom: false);
                                  };
        person.SaveFormAction = () =>
                                {
                                    person.PasswordCheckAction!.Invoke(obj: counter++);
                                };
    }
}
