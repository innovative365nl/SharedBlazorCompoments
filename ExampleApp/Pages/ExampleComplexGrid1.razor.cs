using System.Diagnostics.CodeAnalysis;
using ExampleApp.Components;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;
using Radzen;

namespace ExampleApp.Pages;

public partial class ExampleComplexGrid1
(
    IAttributeState state,
    IInnovativeSidePanelService sidePanelService,
    DialogService dialogService
)
{
    // This is the datasource for the grid
    private IEnumerable<AttributesGridModel>? _attributesView;

    protected override async Task OnInitializedAsync() => await OnRefreshData().ConfigureAwait(true);

    // RadzenButton 🔄 Click
    internal protected async Task OnRefreshData()
    {
            await state
                .RefreshDataAsync()
                .ConfigureAwait(true);

            _attributesView = state.Attributes.Select(selector: AttributesGridModel.ToGridModel).ToArray();
            StateHasChanged();
    }

    // RadzenButton ➕ Click
    private async Task OnCreateAttribute()
    {
        // Define a new instance to be used by the InnovativeForm, which is invoked by sidePanelService.OpenInEditMode
        // (or InnovativeDetail when opened in sidePanelService.OpenInDisplayMode).
        var formModel = new AttributeFormModel { AttributeValue = string.Empty, AttributeType = state.AttributeTypes[0] };
        // Then define what to do with this instance when the "Save" button is clicked on the form.
        formModel.SaveFormAction = async () =>
                                   {
                                       var model = AttributeFormModel.ToDomainModel(formModel);
                                       await state
                                             .CreateAttributeAsync(model)
                                             .ConfigureAwait(true);
                                       await OnRefreshData()
                                             .ConfigureAwait(true);
                                   };
        // And then open de side panel.
        await sidePanelService
            .OpenInEditMode<AttributeFormModel>(formModel)
            .ConfigureAwait(true);
    }

    private async Task OnRowSelected(IEnumerable<AttributesGridModel> models)
    {
        var firstObject = models.FirstOrDefault();
        if (firstObject == null)
        {
            return;
        }

        var attributeModel = state.Attributes.Single(e => e.Id == firstObject.PropertyId);
        // Get the instance to be used by the InnovativeDetail when opened in sidePanelService.OpenInDisplayMode
        // (or InnovativeForm, which is invoked by sidePanelService.OpenInEditMode).
        var formModel = AttributeFormModel.ToFormModel(attributeModel);
        // Then define what to do with this instance when the "Save" button is clicked
        formModel.SaveFormAction = async () =>
                                   {
                                       var model = AttributeFormModel.ToDomainModel(formModel);
                                       await state
                                             .UpdateAttributeAsync(model)
                                             .ConfigureAwait(true);
                                       await OnRefreshData()
                                           .ConfigureAwait(true);
                                   };
        // or the delete button is clicked on the form.
        formModel.DeleteFormAction = async () =>
                                     {
                                         var message = $"Do you want to remove property {formModel.AttributeType.Value} {formModel.AttributeValue}?";
                                         var options = new ConfirmOptions {OkButtonText = "Yes", CancelButtonText = "No"};

                                         var isOk = await dialogService
                                                            .Confirm(message, "Delete Property?", options)
                                                            .ConfigureAwait(true)
                                                   ?? false;

                                         if (isOk)
                                         {
                                             var model = state.Attributes.Single(e => e.Id == formModel.AttributeId);
                                             await state
                                                   .DeleteAttributeAsync(model.Id)
                                                   .ConfigureAwait(true);
                                             await OnRefreshData()
                                                  .ConfigureAwait(true);
                                         }
                                     };
        // And then open de side panel.
        await sidePanelService
              .OpenInDisplayMode(formModel, showDelete: true)
              .ConfigureAwait(true);
    }
}


// This type defines the viewmodel for the grid.
[UIGridClass(AllowSorting = true)]
public sealed class AttributesGridModel
{
    [UIGridField(Name = "Property")]
    public required string PropertyName { get; set; }

    public Guid PropertyId { get; init; }

    [UIGridField(Name = "Value")]
    public required string PropertyValue { get; set; }

    [UIGridField(Name = "Active", CustomComponentType = typeof(CustomBooleanStyle))]
    public bool IsActive { get; set; } 

    public static AttributesGridModel ToGridModel([NotNull]AttributeModel instance)
    {
        return new AttributesGridModel
        {
            PropertyName = instance.Type.Value,
            PropertyId = instance.Id,
            PropertyValue = instance.Name,
            IsActive = instance.IsActive
        };
    }
}

// This type defines the viewmodel for the side panel form,
// which uses the InnovativeDetail when opened in sidePanelService.OpenInDisplayMode
// or then InnovativeForm, which is invoked by sidePanelService.OpenInEditMode.
[UIFormClass("Property Details")]
public sealed class AttributeFormModel : FormModel
{
    private const string PropertyColumnName = "Property";
    private const string ValueColumnName = "Value";

    public AttributeFormModel()
    {
        AddViewColumn(
            name: PropertyColumnName,
            width: 1,
            order: 1,
            offset: 0
        );
        AddViewColumn(
            name: ValueColumnName,
            width: 1,
            order: 2,
            offset: 0
        );
    }

    // This property renders as a custom component in the form of a dropdown.
    [UIFormField(name: "Property", ColumnGroup = PropertyColumnName, FormComponent = typeof(AttributeTypePicker), TextProperty = nameof(AttributeTypeModel.Value))]
    public required AttributeTypeModel AttributeType { get; set; }

    // Not visible on form
    public Guid AttributeId { get; set; }

    [UIFormField(name: "Value", ColumnGroup = ValueColumnName)]
    public required string AttributeValue { get; set; }

    [UIFormField(name: "Active", DisplayComponent = typeof(CustomBooleanStyle))]
    public bool IsActive { get; set; }

    public static AttributeFormModel ToFormModel([NotNull]AttributeModel instance)
    {
        return new AttributeFormModel
        {
            AttributeId = instance.Id,
            AttributeValue = instance.Name,
            AttributeType = instance.Type,
            IsActive = instance.IsActive,
        };
    }

    public static AttributeModel ToDomainModel([NotNull]AttributeFormModel instance) => new AttributeModel
        (
          Id: instance.AttributeId,
          Name: instance.AttributeValue,
          Type: instance.AttributeType,
          IsActive: instance.IsActive
        );
}
