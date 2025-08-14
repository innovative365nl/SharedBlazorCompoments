using ExampleApp.Pages;
using Innovative.Blazor.Components.Components;

namespace ExampleApp.Components;

public partial class AttributeTypePicker(IAttributeState state) : CustomComponent<AttributeTypeModel>
{
    private IEnumerable<AttributeTypeModel> _attributeTypes = [];

    protected override async Task OnInitializedAsync()
    {
        await state.RefreshDataAsync().ConfigureAwait(true);
        _attributeTypes = state.AttributeTypes;
    }

    protected override void OnParametersSet()
    {
        // No internal state needed; Value is always from parent
    }

    private void OnSelectedItemChanged(int? value)
    {
        var selectedType = _attributeTypes.SingleOrDefault(x => x.Id == value);
        if (selectedType != null)
        {
            Value = selectedType;
            OnValueChanged();
        }
    }
}
