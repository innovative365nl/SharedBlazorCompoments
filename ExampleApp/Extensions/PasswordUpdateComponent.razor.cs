using Innovative.Blazor.Components.Components.Form;
using Microsoft.AspNetCore.Components;
using ExampleApp.Pages;

namespace ExampleApp.Extensions;

public partial class PasswordUpdateComponent : ComponentBase, IDynamicBaseComponent
{
    private const int ResultValue = 123;
    [Parameter] public object? Model { get; set; }
    [Parameter] public string? ActionProperty { get; set; }
    [Parameter]public EventCallback<int> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public RightSideDialog<PersonModel>? ParentDialog { get; set; }

    public async Task OnSubmitPressed()
    {
        var personModel = (PersonModel)Model!;
    //    personModel.ControlePasswordAction.Invoke();
    personModel.ControlePasswordAction!.Invoke(1);
        await OnSave.InvokeAsync().ConfigureAwait(false);
    }
    public  Task OnCancelPressed()
    {
        ParentDialog!.SetCustomDialog(false);
        return Task.CompletedTask;
    }


    protected override void OnInitialized()
    {
        if (ParentDialog != null)
        {
            ParentDialog.SetFormComponent(this);
        }
        
        base.OnInitialized();
    }
}
