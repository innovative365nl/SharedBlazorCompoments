using System.Security.Cryptography;
using ExampleApp.Components;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;

namespace ExampleApp.Pages;

public partial class ExampleGridWithPreview(IInnovativeSidePanelService sidePanelService)
{
    private readonly string[] firstNames = ["Jan", "Jaap", "Piet", "Kees", "Tom"];

    private readonly string[] lastNames = ["Appelboom", "Perenboom", "Kersenboom", "Kerstboom"];

    private readonly List<PersonPreviewGridModel> items = [];

    protected override void OnInitialized()
    {
        var data = Enumerable.Range(start: 1, count: 10)
                             .Select(selector: i => new PersonPreviewModel
                                                    {
                                                        Id = Guid.NewGuid(),
                                                        FirstName = firstNames[RandomNumberGenerator.GetInt32(toExclusive: firstNames.Length)],
                                                        LastName = lastNames[RandomNumberGenerator.GetInt32(toExclusive: lastNames.Length)]
                                                    })
                             .ToList();

        items.AddRange(collection: data.Select(selector: PersonPreviewGridModel.ToGridModel));
    }

    private async Task OnRowSelected(IEnumerable<PersonPreviewGridModel> obj)
    {
        PersonPreviewGridModel? rowItem = obj.FirstOrDefault();
        if (rowItem != null)
        {
            var model = PersonPreviewFormModel.ToFormModel(instance: PersonPreviewGridModel.ToModel(instance: rowItem));
            model.SaveFormAction = () =>
                                   {
                                       var item = items.Single(x => x.Id == model.Id);
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

public record PersonPreviewModel
{
    public Guid Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public override string ToString() => $"{FirstName} {LastName}";
}

[UIGridClass(AllowSorting = true)]
public sealed class PersonPreviewGridModel
{
    public Guid Id { get; set; }

    [UIGridField(Name = "Voornaam")]
    public string? FirstName { get; set; }

    [UIGridField(Name = "Achternaam")]
    public string? LastName { get; set; }

    public static PersonPreviewGridModel ToGridModel(PersonPreviewModel instance)
    {
        return new PersonPreviewGridModel
               {
                   Id = instance?.Id ?? Guid.NewGuid(),
                   FirstName = instance?.FirstName,
                   LastName = instance?.LastName
               };
    }
    public static PersonPreviewModel ToModel(PersonPreviewGridModel instance)
    {
        return new PersonPreviewModel
               {
                   Id = instance?.Id ?? Guid.NewGuid(),
                   FirstName = instance?.FirstName,
                   LastName = instance?.LastName
               };
    }
}

public sealed class PersonPreviewFormModel : FormModel
{
    private const string ColumnGroup1 = "PropertyColumn1";

    public PersonPreviewFormModel()
    {
        AddViewColumn(
                      name: ColumnGroup1,
                      width: 1,
                      order: 1,
                      offset: 0
                     );

    }
    public Guid Id { get; set; }

    [UIFormField(name: "Voornaam", ShouldNotifyChanges = true, ColumnGroup = ColumnGroup1)]
    public string? FirstName { get; init; }

    [UIFormField(name: "Achternaam", ShouldNotifyChanges = true, ColumnGroup = ColumnGroup1)]
    public string? LastName { get; init; }

    [UIFormField(name: "Naam", FormComponent = typeof(FullNamePreviewComponent), ColumnGroup = ColumnGroup1)]
    public PersonPreviewModel? FullName { get; init; }

    public static PersonPreviewFormModel ToFormModel(PersonPreviewModel instance)
    {
        return new PersonPreviewFormModel
               {
                   Id = instance?.Id ?? Guid.NewGuid(),
                   FirstName = instance?.FirstName,
                   LastName = instance?.LastName,
                   FullName = instance
               };
    }
}
