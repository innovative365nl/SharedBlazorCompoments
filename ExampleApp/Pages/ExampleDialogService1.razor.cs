using Innovative.Blazor.Components.Services;
using Microsoft.Kiota.Abstractions;

namespace ExampleApp.Pages;

public partial class ExampleDialogService1(IInnovativeSidePanelService sidePanelService)
{
    private readonly List<string> actionLog = [];

    private SimplePersonModel person = CreatePerson();

    protected override void OnInitialized()
    {
        person.SaveFormAction = async () =>
                                {
                                    LogAction("Saving started");
                                    person.ReportProgress("Connecting to server...");
                                    await Task.Delay(500).ConfigureAwait(true);
                                    person.ReportProgress("Uploading data...");
                                    await Task.Delay(1000).ConfigureAwait(true);
                                    person.ReportProgress("Processing on server (step 1/3)...");
                                    await Task.Delay(800).ConfigureAwait(true);
                                    person.ReportProgress("Processing on server (step 2/3)...");
                                    await Task.Delay(800).ConfigureAwait(true);
                                    person.ReportProgress("Finalizing...");
                                    await Task.Delay(600).ConfigureAwait(true);
                                    throw new ApiException("test");
                                    //    LogAction("Model saved");
                                };
        person.DeleteFormAction = () =>
                                  {
                                      person = new SimplePersonModel();
                                      var logEntry = "Model deleted";
                                      LogAction(message: logEntry);
                                      return Task.CompletedTask;
                                  };
        person.CancelFormAction = () =>
                                  {
                                      person = CreatePerson();
                                      var logEntry = "Model canceled";
                                      LogAction(message: logEntry);
                                      return Task.CompletedTask;
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
    }

    private async Task OpenPersonDialog()
    {
        await sidePanelService
              .OpenInDisplayMode(model: person, showDelete: true, dataTestId: "12345678900987654321") // person is passed by reference so after save
              .ConfigureAwait(continueOnCapturedContext: true);                                       // you'll have the updated model

        StateHasChanged();
    }
}
