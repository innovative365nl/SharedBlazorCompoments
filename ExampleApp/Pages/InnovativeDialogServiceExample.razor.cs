using ExampleApp.Components;
using ExampleApp.Extensions;
using ExampleApp.Translations;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;
using PasswordUpdateComponent = ExampleApp.Components.PasswordUpdateComponent;

namespace ExampleApp.Pages;

public partial class InnovativeDialogServiceExample(IInnovativeSidePanelService sidePanelService)
{
    private PersonDisplayModel personModel = new()
    {
        FirstName = "John",
        LastName = "Doe",
        IsActive = true,
        BirthDate = new DateTime(1993, 5, 12),
        ComplexComponent =  new()
        {
            Name = "Complex Component",
            Description = "This is a complex component"
        }
    };

    private readonly List<string> actionLog = [];

    protected override void OnInitialized()
    {
        personModel.UpdatePasswordAction = count =>
        {
            var logEntry = $"Password updated:{count} times";
            LogAction(logEntry);
        };

        personModel.PasswordCheckAction = isValid =>
        {
            var logEntry = $"Password checked. Is valid: {isValid}";
            LogAction(logEntry);
        };

        personModel.SaveFormAction = () =>
        {
            var logEntry = "Model saved";
            LogAction(logEntry);
        };
        personModel.DeleteFormAction = () =>
        {
           var logEntry = "Model deleted";
           LogAction(logEntry);
        };
        personModel.CancelFormAction = () =>
        {
            var logEntry = "Model canceled";
            LogAction(logEntry);
        };

        base.OnInitialized();
    }

    private void LogAction(string message)
    {
        var logEntry = $"{DateTime.Now:HH:mm:ss.fff}: {message}";
        actionLog.Add(logEntry);
        Console.WriteLine(logEntry);
        StateHasChanged();
    }

    private async Task OpenPersonDialog()
    {
        var result = await sidePanelService
                                       .OpenDynamicFormDialog(personModel)
                                       .ConfigureAwait(false);
        personModel = result;
        StateHasChanged();
    }

    private async Task OpenNewPersonDialog()
    {
        var result = await sidePanelService
                           .OpenDynamicFormDialog<PersonDisplayModel>()
                           .ConfigureAwait(false);

        personModel = result;
        StateHasChanged();
    }
}

[UIFormClass(
    title: nameof(Example.DialogService_Person),
    ResourceType = typeof(Example),
    ColumnOrder = ["Name", "EmployeeInfo", "Description"],
    ColumnWidthNames = ["Name", "EmployeeInfo", "Description"],
    ColumnWidthValues = [1, 1, 3]
)]
public class PersonDisplayModel : DisplayFormModel
{
    [UIFormField(name: "First Name", ColumnGroup = "Name")]
    public string? FirstName { get; set; }

    [UIFormField(name: "Last Name", ColumnGroup = "Name")]
    public string? LastName { get; set; }

    [UIFormField(name: "Is Active", DisplayComponent = typeof(CustomBooleanStyle), FormComponent = typeof(CustomBooleanStyle), ColumnGroup = "EmployeeInfo")]
    public bool IsActive { get; set; }

    [UIFormField(name: "Birth Date",  ColumnGroup = "EmployeeInfo")]
    public DateTime? BirthDate { get; set; }

    [UIFormField(name : "Description", UseWysiwyg = true, ColumnGroup = "Description")]
    public string? Description { get; set; }

    [UIFormViewAction(name: "Update Password", Order = 1, CustomComponent = typeof(PasswordUpdateComponent))]
    public Action<int>? UpdatePasswordAction { get; set; }

    [UIFormViewAction(name: "Check Password", Order = 1, CustomComponent = typeof(PasswordCheckComponent))]
    public Action<bool>? PasswordCheckAction { get; set; }

    [UIFormField(name: "Complex Component", ColumnGroup = "Description", FormComponent = typeof(ComplexComponent), TextProperty = nameof(ComplexComponent.Description))]
    public ComplexModel? ComplexComponent { get; set; } = new()
    {
        Name = "Complex Component",
        Description = "This is a complex component"
    };
}
