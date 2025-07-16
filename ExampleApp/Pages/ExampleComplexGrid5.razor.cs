using System.Security.Cryptography;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;

namespace ExampleApp.Pages;

public partial class ExampleComplexGrid5(IInnovativeSidePanelService sidePanelService)
{
    private readonly string[] firstNames = ["Jan", "Jaap", "Piet", "Kees", "Tom"];

    private readonly List<Person5GridModel> items = [];

    private readonly string[] lastNames = ["Appelboom", "Perenboom", "Kersenboom", "Kerstboom", "Pruimenboom"];

    protected override void OnInitialized()
    {
        var data = Enumerable.Range(start: 1, count: 10)
                             .Select(selector: i => new Person5Model
                                                    {
                                                        Id = Guid.NewGuid()
                                                      , FirstName = firstNames[RandomNumberGenerator.GetInt32(toExclusive: firstNames.Length)]
                                                      , LastName = lastNames[RandomNumberGenerator.GetInt32(toExclusive: lastNames.Length)]
                                                      , DateOfBirth = new DateTime(year: (RandomNumberGenerator.GetInt32(toExclusive: 50) + 1950)
                                                                                 , month: (RandomNumberGenerator.GetInt32(toExclusive: 11) + 1)
                                                                                 , day: (RandomNumberGenerator.GetInt32(toExclusive: 27)   + 1))
                                                    })
                             .ToList();

        items.AddRange(collection: data.Select(selector: Person5GridModel.ToGridModel));
    }

    private async Task OnRowSelected(IEnumerable<Person5GridModel> obj)
    {
        Person5GridModel? rowItem = obj.FirstOrDefault();
        if (rowItem != null)
        {
            var model = Person5FormModel.ToFormModel(instance: Person5GridModel.ToModel(instance: rowItem));
            model.SaveFormAction = () =>
                                   {
                                       Person5GridModel item = items.Single(predicate: x => x.Id == model.Id);
                                       item.FirstName = model.FirstName;
                                       item.LastName = model.LastName;
                                       return Task.CompletedTask;
                                   };

            await sidePanelService
                  .OpenInEditMode(model: model)
                  .ConfigureAwait(continueOnCapturedContext: true);
        }
    }
}

public class Person5Model
{
    public Guid Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public override string ToString() => $"{FirstName} {LastName}";
}

[UIGridClass(AllowSorting = true)]
public sealed class Person5GridModel
{
    public Guid Id { get; set; }

    [UIGridField(Name = "Voornaam")]
    public string? FirstName { get; set; }

    [UIGridField(Name = "Achternaam")]
    public string? LastName { get; set; }

    [UIGridField(Name = "Geboortedatum")]
    public DateTime? DateOfBirth { get; set; }

    public static Person5GridModel ToGridModel(Person5Model instance)
    {
        return new Person5GridModel
               {
                   Id = instance?.Id ?? Guid.NewGuid()
                 , FirstName = instance?.FirstName
                 , LastName = instance?.LastName
                 , DateOfBirth = instance?.DateOfBirth
               };
    }
    public static Person5Model ToModel(Person5GridModel instance)
    {
        return new Person5Model
               {
                   Id = instance?.Id ?? Guid.NewGuid()
                 , FirstName = instance?.FirstName
                 , LastName = instance?.LastName
                 , DateOfBirth = instance?.DateOfBirth
               };
    }
}

public sealed class Person5FormModel : FormModel
{
    private const string ColumnGroup1 = "PropertyColumn1";

    public Person5FormModel() => AddViewColumn(name: ColumnGroup1, width: 12, order: 1, offset: 0);
    public Guid Id { get; set; }

    [UIFormField(name: "Voornaam", ColumnGroup = ColumnGroup1)]
    public string? FirstName { get; init; }

    [UIFormField(name: "Achternaam", ColumnGroup = ColumnGroup1)]
    public string? LastName { get; init; }

    [UIFormField(name: "Geboortedatum", ColumnGroup = ColumnGroup1, FormParameters = ["DateFormat=yyyy-MM-dd"])]
    public DateTime? DateOfBirth { get; set; }

    [UIFormField(name: "Leeftijd", ColumnGroup = ColumnGroup1)]
    public int? Age => CalculateAge(dateOfBirth: DateOfBirth);

    public static int? CalculateAge(DateTime? dateOfBirth)
    {
        if (dateOfBirth is null)
            return null;

        DateTime today = DateTime.Today;
        int result = today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > today.AddYears(value: -result))
        {
            result--;
        }

        return result;
    }

    public static Person5FormModel ToFormModel(Person5Model instance)
    {
        return new Person5FormModel
               {
                   Id = instance?.Id ?? Guid.NewGuid()
                 , FirstName = instance?.FirstName
                 , LastName = instance?.LastName
                 , DateOfBirth = instance?.DateOfBirth
               };
    }
}
