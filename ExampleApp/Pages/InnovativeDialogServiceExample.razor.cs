using ExampleApp.Components;
using ExampleApp.Extensions;
using ExampleApp.Translations;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Pages;


public partial class InnovativeDialogServiceExample(IInnovativeDialogService dialogService)
{


    private PersonModel person = new()
    {
        FirstName = "John",
        LastName = "Doe",
        IsActive = true,
        BirthDate = new DateTime(1993,
            5,
            12),
        ComplexComponent =  new()
        {
            Name = "Complex Component",
            Description = "This is a complex component"
        }
    };

    private List<string> actionLog = new();

    protected override void OnInitialized()
    {
        person.UpdatePasswordAction = id =>
        {
            var logEntry = $"Password updated with ID: {id} at {DateTime.Now:HH:mm:ss}";
            actionLog.Add(logEntry);
            Console.WriteLine(logEntry);
            StateHasChanged();
        };

        person.ControlePasswordAction = result =>
        {
            var logEntry = $"Password control result: {result} at {DateTime.Now:HH:mm:ss}";
            actionLog.Add(logEntry);
            Console.WriteLine(logEntry);
            StateHasChanged();
        };

        base.OnInitialized();
    }

    private async Task OpenPersonDialog()
    {
        var result = await dialogService.OpenDynamicFormDialog(
            person).ConfigureAwait(false);

        person = result;

        StateHasChanged();
    }
    private async Task OpenNewPersonDialog()
    {
        var result = await dialogService.OpenDynamicFormDialog<PersonModel>().ConfigureAwait(false);

        person = result;

        StateHasChanged();
    }
}

[UIFormClass( title: nameof(Example.DialogService_Person),
    ResourceType = typeof(Example), ColumnOrder = new[] {   "Name","EmployeeInfo","Description" },
    ColumnWidthNames = new[] {"Name", "EmployeeInfo", "Description"},
    ColumnWidthValues = new[] {1, 1, 3})]
public class PersonModel
{
    [UIFormField(name: "First Name", ColumnGroup = "Name")]
    public string? FirstName { get; set; }

    [UIFormField(name: "Last Name", ColumnGroup = "Name")]
    public string? LastName { get; set; }

   // [UIFormField(name: "Age", ColumnGroup = "EmployeeInfo")]
    //public int? Age => DateTime.Now.Year - BirthDate!.Value.Year;

    [UIFormField(name: "Is Active", DisplayComponent = typeof(CustomBoolStyle), FormComponent = typeof(CustomBoolStyle), ColumnGroup = "EmployeeInfo")]
    public bool IsActive { get; set; }

    [UIFormField(name: "Birth Date",  ColumnGroup = "EmployeeInfo")]
    public DateTime? BirthDate { get; set; }

    [UIFormField(name : "Description", UseWysiwyg = true, ColumnGroup = "Description")]
    public string? Description { get; set; }

    [UIFormViewAction(Name = "Update Password", Order = 1)]
    public  Action<int>? UpdatePasswordAction { get; set; }

    [UIFormViewAction(Name = "Control Password", Order = 1, CustomComponent = typeof(PasswordUpdateComponent))]
    public   Action<int>? ControlePasswordAction { get; set; }

    [UIFormField(name: "Complex Component", ColumnGroup = "Description", FormComponent = typeof(ComplexComponent), TextProperty = "Description")]
    public ComplexModel? ComplexComponent { get; set; } = new()
    {
        Name = "Complex Component",
        Description = "This is a complex component"
    };
}


public class ComplexModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
