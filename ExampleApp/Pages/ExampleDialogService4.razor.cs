using ExampleApp.Components;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Pages;

public partial class ExampleDialogService4(IInnovativeSidePanelService panelService) : ComponentBase
{
    private PersonExampleModel person = null!;

    protected override void OnInitialized()
    {
        person = new PersonExampleModel()
                 {
                     FirstName = "John",
                     LastName = "Doe",
                     IsActive = true,
                     BirthDate = new DateTime(1993, 5, 12),
                 };
    }

    private async Task OpenInEditMode() => await panelService.OpenInEditMode(person).ConfigureAwait(false);

}

internal sealed class PersonExampleModel : FormModel
{

    [UIFormField(name: "First Name", ColumnGroup = "Name")]
    public string? FirstName { get; set; }

    [UIFormField(name: "Last Name", ColumnGroup = "Name", FormComponent = typeof(CustomPrimitiveTypeComponent))]
    public string? LastName { get; set; }

    [UIFormField(name: "Birth Date",  ColumnGroup = "EmployeeInfo", DisplayParameters = ["Format={0:dddd d MMMM yyyy}"], FormParameters = ["DateFormat=yyyy-MM-dd"])]
    public DateTime? BirthDate { get; set; }

    [UIFormField(name: "Is Active", DisplayComponent = typeof(CustomBooleanStyle), FormComponent = typeof(CustomBooleanStyle), ColumnGroup = "EmployeeInfo")]
    public bool IsActive { get; set; }

    [UIFormField(name : "Description", UseWysiwyg = true, ColumnGroup = "Description")]
    public string? Description { get; set; }
}
