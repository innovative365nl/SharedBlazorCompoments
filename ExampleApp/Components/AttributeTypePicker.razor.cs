using ExampleApp.Pages;
using Innovative.Blazor.Components.Components;

namespace ExampleApp.Components;

public partial class AttributeTypePicker(IAttributeState state) : CustomComponent<AttributeTypeModel>
{
    private IEnumerable<AttributeTypeModel> _attributeTypes = [];
    private int? _attributeTypeId;

    protected override async Task OnInitializedAsync()
    {
        await state.RefreshDataAsync().ConfigureAwait(true);
        _attributeTypes = state.AttributeTypes;
    }

    protected override void OnParametersSet()
    {
        _attributeTypeId = Value == null
                               ? _attributeTypes.OrderBy(x => x.Value).FirstOrDefault()?.Id
                               : _attributeTypes.SingleOrDefault(x => x.Id == Value.Id)?.Id;
    }

    private void OnSelectedItemChanged()
    {
        Value = _attributeTypeId == null
                    ? _attributeTypes.OrderBy(x => x.Value).FirstOrDefault()
                    : _attributeTypes.SingleOrDefault(x => x.Id == _attributeTypeId);

        // Create a backup of the current binding value (_attributeTypeId)
        var backup = _attributeTypeId;
        // because after a
        OnValueChanged();
        // the Value and _attributeTypeId are reset to their original values
        _attributeTypeId = backup;
    }
}
