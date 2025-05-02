using Innovative.Blazor.Components.Services;

namespace ExampleApp.Pages;

public partial class ExampleDialogService1(IInnovativeSidePanelService sidePanelService)
{
    private readonly List<string> actionLog = [];

    private SimplePersonModel person = CreatePerson();

    private bool isSaved;
    private bool isDeleted;
    private bool isCanceled;

    protected override void OnInitialized()
    {
        person.SaveFormAction = () =>
                                {
                                    isSaved = true;
                                    var logEntry = "Model saved";
                                    LogAction(message: logEntry);
                                };
        person.DeleteFormAction = () =>
                                  {
                                      isDeleted = true;
                                      var logEntry = "Model deleted";
                                      LogAction(message: logEntry);
                                  };
        person.CancelFormAction = () =>
                                  {
                                      isCanceled = true;
                                      var logEntry = "Model canceled";
                                      LogAction(message: logEntry);
                                  };

        base.OnInitialized();
    }

    private static SimplePersonModel CreatePerson()
    {
        return new SimplePersonModel
               {
                   FirstName = "John"
                 , LastName = "Doe"
                 , IsActive = true
                 , BirthDate = new DateTime(year: 1993, month: 5, day: 12)
                 , Biography = "This is a biography of John Doe. He is a software engineer with over 10 years of experience in the industry."
               };
    }

    private void LogAction(string message)
    {
        var logEntry = $"{DateTime.Now:HH:mm:ss.fff}: {message}";
        actionLog.Add(item: logEntry);
        Console.WriteLine(value: logEntry);
        StateHasChanged();
    }

    private async Task OpenPersonDialog()
    {
        SimplePersonModel result = await sidePanelService
                                         .OpenDynamicFormDialog(model: person, showDelete:true)
                                         .ConfigureAwait(continueOnCapturedContext: false);

        if (isSaved)
        {
            person = result;
        }
        if (isCanceled)
        {
            person = CreatePerson();
        }
        if (isDeleted)
        {
            person = new SimplePersonModel();
        }

        StateHasChanged();

        isSaved = false;
        isCanceled = false;
        isDeleted = false;
    }
}
