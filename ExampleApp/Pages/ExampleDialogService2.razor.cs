using ExampleApp.Components;
using ExampleApp.Translations;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Enumerators;
using Innovative.Blazor.Components.Services;
using PasswordUpdateComponent = ExampleApp.Components.PasswordUpdateComponent;

namespace ExampleApp.Pages;

public partial class ExampleDialogService2(IInnovativeSidePanelService sidePanelService)
{
    private PersonFormModel? person;

    private readonly List<string> actionLog = [];

    protected override void OnInitialized() => person = CreatePerson();

    private PersonFormModel CreatePerson()
    {
        var result = new PersonFormModel
                     { FirstName = "John"
                     , LastName = "Doe"
                     , IsActive = true
                     , BirthDate = new DateTime(year: 1993, month: 5, day: 12)
                     , ComplexComponent = new ComplexModel
                                          {
                                              Name = "Complex Component"
                                            , Description = "This is a complex component"
                                          }
                     , UpdatePasswordAction = count =>
                                              {
                                                  var logEntry = $"Password updated:{count} times";
                                                  LogAction(message: logEntry);
                                              }
                     , PasswordCheckAction = isValid =>
                                             {
                                                 var logEntry = $"Password checked. Is valid: {isValid}";
                                                 LogAction(message: logEntry);
                                             }
                     , SaveFormAction = () =>
                                        {
                                            var logEntry = "Model saved";
                                            LogAction(logEntry);
                                            return Task.CompletedTask;
                                        }
                     , DeleteFormAction = () =>
                                          {
                                              if(person is not null)
                                              {
                                                  person.FirstName = null;
                                                  person.LastName = null;
                                                  person.IsActive = true;
                                                  person.BirthDate = null;
                                              }
                                              var logEntry = "Model deleted";
                                              LogAction(message: logEntry);
                                              return Task.CompletedTask;
                                          }
                     , CancelFormAction = () =>
                                          {
                                              person = CreatePerson();
                                              var logEntry = "Model canceled";
                                              LogAction(message: logEntry);
                                              return Task.CompletedTask;
                                          }
                     };

        return result;
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
        person ??= CreatePerson();
        await sidePanelService
              .OpenInDisplayMode(person)
              .ConfigureAwait(false);
    }

    private async Task OpenNewPersonDialog()
    {
        var newPerson = new PersonFormModel
                        { IsActive = true
                        , UpdatePasswordAction = count =>
                                                 {
                                                     var logEntry = $"Password updated:{count} times";
                                                     LogAction(message: logEntry);
                                                 }
                        , PasswordCheckAction = isValid =>
                                                {
                                                    var logEntry = $"Password checked. Is valid: {isValid}";
                                                    LogAction(message: logEntry);
                                                }
        };

        await sidePanelService
                           .OpenInEditMode(newPerson)
                           .ConfigureAwait(true);
    }

    private async Task OpenLargeWidthDialog()
    {
        person ??= CreatePerson();
        await sidePanelService
              .OpenInDisplayMode(person, width: SideDialogWidth.Large)
              .ConfigureAwait(false);
    }

    private async Task OpenExtraLargeWidthDialog()
    {
        person ??= CreatePerson();
        await sidePanelService
              .OpenInDisplayMode(person, width: SideDialogWidth.ExtraLarge)
              .ConfigureAwait(false);
    }
}

[UIFormClass(title: nameof(Example.Person), ResourceType = typeof(Example))]
public class PersonFormModel : FormModel
{
    private const string NameColumn = "Name";
    private const string EmployeeInfoColumn = "EmployeeInfo";
    private const string DescriptionColumn = "Description";

    public PersonFormModel()
    {
        AddViewColumn(NameColumn, 1, 6, 0);;
        AddViewColumn(EmployeeInfoColumn, 1, 6, 0);
        AddViewColumn(DescriptionColumn, 1, 12, 0);;
    }
    [UIFormField(name: "First Name", ColumnGroup = "Name")]
    public string? FirstName { get; set; }

    [UIFormField(name: "Last Name", ColumnGroup = "Name")]
    public string? LastName { get; set; }

    [UIFormField(name: "Birth Date",  ColumnGroup = "EmployeeInfo", DisplayParameters = ["Format={0:dddd d MMMM yyyy}"], FormParameters = ["DateFormat=yyyy-MM-dd"])]
    public DateTime? BirthDate { get; set; }

    [UIFormField(name: "Is Active", DisplayComponent = typeof(CustomBooleanStyle), FormComponent = typeof(CustomBooleanStyle), ColumnGroup = "EmployeeInfo")]
    public bool IsActive { get; set; }

    [UIFormField(name : "Description", UseWysiwyg = true, ColumnGroup = "Description")]
    public string? Description { get; set; }

    [UIFormField(name: "Complex Component",
                 ColumnGroup = "Description",
                 FormComponent = typeof(ComplexEditComponent),
                 FormParameters = ["DisplayLabel=false"], // There must be a parameter named DisplayLabel in the ComplexEditComponent
                 DisplayComponent = typeof(ComplexDisplayComponent),
                 DisplayParameters = ["DisplayLabel=false"], // There must be a parameter named DisplayLabel in the ComplexDisplayComponent
                 TextProperty = nameof(ComplexComponent.Description)
    )]
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
