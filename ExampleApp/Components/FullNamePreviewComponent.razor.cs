using ExampleApp.Pages;
using Innovative.Blazor.Components.Components;

namespace ExampleApp.Components;

public partial class FullNamePreviewComponent : CustomComponent<PersonPreviewModel>
{
    public new PersonPreviewModel? Value
    {
        get => base.Value;
        set
        {
            if (base.Value != value)
            {
                base.Value = value;
                ValueChanged.InvokeAsync(arg: value);
                StateHasChanged();
            }
        }
    }

    public override void OnFormValueChanged(KeyValuePair<string, object?> pair)
    {
        if (Value != null)
        {
            if (pair is {Key: nameof(PersonPreviewModel.FirstName), Value: string firstName})
            {
                Value.FirstName = firstName;
            }
            if (pair is {Key: nameof(PersonPreviewModel.LastName), Value: string lastName})
            {
                Value.LastName = lastName;
            }
        }
    }
}
