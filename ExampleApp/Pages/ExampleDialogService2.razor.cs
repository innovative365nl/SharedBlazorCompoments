using ExampleApp.Components;
using ExampleApp.Translations;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;
using PasswordUpdateComponent = ExampleApp.Components.PasswordUpdateComponent;

namespace ExampleApp.Pages;

public partial class ExampleDialogService2(IInnovativeSidePanelService sidePanelService)
{
    private PersonModel person = CreatePerson();

    private readonly List<string> actionLog = [];

    protected override void OnInitialized()
    {
        person.UpdatePasswordAction = count =>
        {
            var logEntry = $"Password updated:{count} times";
            LogAction(logEntry);
        };

        person.PasswordCheckAction = isValid =>
        {
            var logEntry = $"Password checked. Is valid: {isValid}";
            LogAction(logEntry);
        };

        person.SaveFormAction = () =>
        {
            var logEntry = "Model saved";
            LogAction(logEntry);
            return Task.CompletedTask;
        };
        person.DeleteFormAction = () =>
        {
           person = new PersonModel { IsActive = true };
           var logEntry = "Model deleted";
           LogAction(logEntry);
           return Task.CompletedTask;
        };
        person.CancelFormAction = () =>
        {
            person = CreatePerson();
            var logEntry = "Model canceled";
            LogAction(logEntry);
            return Task.CompletedTask;
        };

        base.OnInitialized();
    }

    private static PersonModel CreatePerson()
    {
        return new PersonModel
               {
                   FirstName = "John",
                   LastName = "Doe",
                   IsActive = true,
                   BirthDate = new DateTime(1993, 5, 12),
                   ComplexComponent = new()
                                      {
                                          Name = "Complex Component",
                                          Description = "This is a complex component"
                                      }
               };
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
        await sidePanelService
               .OpenInDisplayMode(person)
               .ConfigureAwait(false);
    }

    private async Task OpenNewPersonDialog()
    {
        person = new PersonModel { IsActive = true };

        await sidePanelService
                           .OpenInEditMode<PersonModel>(person)
                           .ConfigureAwait(true);
    }
}

[UIFormClass(title: nameof(Example.DialogService_Person), ResourceType = typeof(Example))]
public class PersonModel : FormModel
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

    [UIFormField(name: "Complex Component", ColumnGroup = "Description", FormComponent = typeof(ComplexComponent),
                 TextProperty = nameof(ComplexComponent.Description))]
    public ComplexModel? ComplexComponent { get; set; } = new()
    {
        Name = "Complex Component",
        Description = "This is a complex component"
    };

    [UIFormViewAction(name: "Update Password", Order = 1, CustomComponent = typeof(PasswordUpdateComponent))]
    public Action<int>? UpdatePasswordAction { get; set; }

    [UIFormViewAction(name: "Check Password", Order = 1, CustomComponent = typeof(PasswordCheckComponent))]
    public Action<bool>? PasswordCheckAction { get; set; }
}
