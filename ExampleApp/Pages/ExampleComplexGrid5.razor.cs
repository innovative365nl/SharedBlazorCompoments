using ExampleApp.Translations;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;

namespace ExampleApp.Pages;

public partial class ExampleComplexGrid5(IInnovativeSidePanelService sidePanelService)
{
    private readonly List<Person5GridModel> items = [];

    protected override void OnInitialized()
    {
        ExampleDataSet.Instance.GenerateTestData(10);
        items.AddRange(collection: ExampleDataSet.Instance.Data.Select(selector: Person5GridModel.ToGridModel));
    }

    private async Task OnRowSelected(IEnumerable<Person5GridModel> obj)
    {
        Person5GridModel? rowItem = obj.FirstOrDefault();
        if (rowItem is null)
            return;

        var model = Person5FormModel.ToFormModel(instance: Person5GridModel.ToModel(instance: rowItem));
        model.SaveFormAction = () =>
                               {
                                   Person5GridModel item = items.Single(predicate: x => x.Id == model.Id);
                                   item.FirstName = model.FirstName;
                                   item.LastName = model.LastName;
                                   item.Income = model.Income;
                                   item.DateOfBirth = model.DateOfBirth?.ToDateString();
                                   return Task.CompletedTask;
                               };

        await sidePanelService
              .OpenInEditMode(model: model)
              .ConfigureAwait(continueOnCapturedContext: true);
    }
}


[UIGridClass(ResourceType = typeof(Example) , AllowSorting = true)]
public sealed class Person5GridModel
{
    public Guid Id { get; set; }

    [UIGridField(Name = "FirstName")]
    public string? FirstName { get; set; }

    [UIGridField(Name = "LastName")]
    public string? LastName { get; set; }

    [UIGridField(Name = "DateOfBirth")]
    public string? DateOfBirth { get; set; }

    [UIGridField(Name = "Income")]
    public decimal? Income { get; set; }

    public static Person5GridModel ToGridModel(PersonModel instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        return new Person5GridModel
               {
                   Id = instance.Id,
                   FirstName = instance.FirstName,
                   LastName = instance.LastName,
                   Income = instance.Income,
                   DateOfBirth = instance.DateOfBirth.ToDateString()
               };
    }
    public static PersonModel ToModel(Person5GridModel instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        return new PersonModel
               {
                   Id = instance.Id,
                   FirstName = instance.FirstName ?? string.Empty,
                   LastName = instance.LastName ?? string.Empty,
                   Income = instance.Income ?? decimal.Zero,
                   DateOfBirth =  (instance.DateOfBirth ?? string.Empty).ToDate()
               };
    }
}

[UIFormClass(title: nameof(Example.Person), ResourceType = typeof(Example))]
public sealed class Person5FormModel : FormModel
{
    private const string ColumnGroup1 = "PropertyColumn1";
    private const string ColumnGroup2 = "PropertyColumn2";
    private const string ColumnGroup3 = "PropertyColumn3";
    private const string ColumnGroup4 = "PropertyColumn4";
    private const string ColumnGroup5 = "PropertyColumn5";

    public Person5FormModel()
    {
        AddViewColumn(name: ColumnGroup1, width: 12, order: 1, offset: 0);
        AddViewColumn(name: ColumnGroup2, width: 6, order: 1, offset: 0);
        AddViewColumn(name: ColumnGroup3, width: 6, order: 1, offset: 0);
        AddViewColumn(name: ColumnGroup4, width: 3, order: 1, offset: 0);
        AddViewColumn(name: ColumnGroup5, width: 12, order: 1, offset: 0);
    }

    public Guid Id { get; set; }

    [UIFormField(name: "FirstName", ColumnGroup = ColumnGroup1)]
    public string? FirstName { get; init; }

    [UIFormField(name: "LastName", ColumnGroup = ColumnGroup1)]
    public string? LastName { get; init; }

    [UIFormField(name: "DateOfBirth", ColumnGroup = ColumnGroup2, FormParameters = [$"DateFormat={Constants.DateFormat}"], DisplayParameters = ["Format={0:dddd d MMMM yyyy}"])]
    public DateTime? DateOfBirth { get; set; }

    [UIFormField(name: "Age", ColumnGroup = ColumnGroup3)]
    public int? Age => DateOfBirth?.Age();

    [UIFormField(name: "Disabled Number", ColumnGroup = ColumnGroup4, FormParameters = ["Disabled=true"])]
    public int? Nummer { get; set; } = 90;

    [UIFormField(name: "Summary", ColumnGroup = ColumnGroup5, DisplayParameters = ["DisplayLabel=false"], FormParameters = ["Disabled=true", "DisplayLabel=false"], UseWysiwyg = true)]
    public string? Summary => $"<h3>This is displayed without a label.</h3><p>{FirstName} {LastName} ({Age}) was born on {DateOfBirth:dddd d MMMM yyyy} and is {Age} years old with a yearly income of {Income:C0}.</p>";

    [UIFormField(name: "Income", ColumnGroup = ColumnGroup2, FormParameters = [$"Format={Constants.CurrencyFormat}"], DisplayParameters = [$"Format={Constants.CurrencyFormat}"])]
    public decimal? Income { get; set; }


    public static Person5FormModel ToFormModel(PersonModel instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        return new Person5FormModel
               {
                   Id = instance.Id,
                   FirstName = instance.FirstName,
                   LastName = instance.LastName,
                   Income = instance.Income,
                   DateOfBirth = instance.DateOfBirth.ToDateTime(TimeOnly.MinValue)
               };
    }
}
