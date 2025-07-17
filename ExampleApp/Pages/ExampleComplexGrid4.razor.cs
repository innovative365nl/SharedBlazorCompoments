using System.Security.Cryptography;
using ExampleApp.Components;
using ExampleApp.Translations;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;

namespace ExampleApp.Pages;

public partial class ExampleComplexGrid4(IInnovativeSidePanelService sidePanelService)
{
    private readonly string[] firstNames = ["Jan", "Jaap", "Piet", "Kees", "Tom"];

    private readonly string[] lastNames = ["Appelboom", "Perenboom", "Kersenboom", "Kerstboom"];

    private readonly List<Person4GridModel> items = [];

    protected override void OnInitialized()
    {
        var data = Enumerable.Range(start: 1, count: 10)
                             .Select(selector: i => new Person4Model
                                                    {
                                                        Id = Guid.NewGuid(),
                                                        FirstName = firstNames[RandomNumberGenerator.GetInt32(toExclusive: firstNames.Length)],
                                                        LastName = lastNames[RandomNumberGenerator.GetInt32(toExclusive: lastNames.Length)]
                                                    })
                             .ToList();

        items.AddRange(collection: data.Select(selector: Person4GridModel.ToGridModel));
    }

    private async Task OnRowSelected(IEnumerable<Person4GridModel> obj)
    {
        Person4GridModel? rowItem = obj.FirstOrDefault();
        if (rowItem != null)
        {
            var model = Person4FormModel.ToFormModel(instance: Person4GridModel.ToModel(instance: rowItem));
            model.SaveFormAction = () =>
                                   {
                                       Person4GridModel item = items.Single(predicate: x => x.Id == model.Id);
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

public record Person4Model : INotifyFormValueChanged
{
    public Guid Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public void OnFormValueChanged(string propertyName, object? value)
    {
        switch (propertyName)
        {
            case nameof(FirstName):
                FirstName = value?.ToString();
                break;
            case nameof(LastName):
                LastName = value?.ToString();
                break;
        }
    }

    public override string ToString() => $"{FirstName} {LastName}";
}

[UIGridClass(AllowSorting = true)]
public sealed class Person4GridModel
{
    public Guid Id { get; set; }

    [UIGridField(Name = "Voornaam")]
    public string? FirstName { get; set; }

    [UIGridField(Name = "Achternaam")]
    public string? LastName { get; set; }

    public static Person4GridModel ToGridModel(Person4Model instance)
    {
        return new Person4GridModel
               {
                   Id = instance?.Id ?? Guid.NewGuid(),
                   FirstName = instance?.FirstName,
                   LastName = instance?.LastName
               };
    }
    public static Person4Model ToModel(Person4GridModel instance)
    {
        return new Person4Model
               {
                   Id = instance?.Id ?? Guid.NewGuid(),
                   FirstName = instance?.FirstName,
                   LastName = instance?.LastName
               };
    }
}

[UIFormClass(title: "Person", ResourceType = typeof(Example))]
public sealed class Person4FormModel : FormModel
{
    private const string ColumnGroup1 = "PropertyColumn1";

    public Person4FormModel() => AddViewColumn(name: ColumnGroup1, width: 1, order: 1, offset: 0);

    public Guid Id { get; set; }

    [UIFormField(name: "Voornaam", ShouldNotifyChanges = true, ColumnGroup = ColumnGroup1)]
    public string? FirstName { get; init; }

    [UIFormField(name: "Achternaam", ShouldNotifyChanges = true, ColumnGroup = ColumnGroup1)]
    public string? LastName { get; init; }

    [UIFormField(name: "Naam", FormComponent = typeof(FullNamePreviewComponent), ColumnGroup = ColumnGroup1)]
    public Person4Model? FullName { get; init; }

    public static Person4FormModel ToFormModel(Person4Model instance)
    {
        return new Person4FormModel
               {
                   Id = instance?.Id ?? Guid.NewGuid(),
                   FirstName = instance?.FirstName,
                   LastName = instance?.LastName,
                   FullName = instance
               };
    }
}
