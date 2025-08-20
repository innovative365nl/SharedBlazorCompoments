using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Bogus;
using ExampleApp.Translations;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;

namespace ExampleApp.Pages;

public partial class ExampleComplexGrid6(IInnovativeSidePanelService sidePanelService)
{
    private InnovativeGrid<Person6GridModel>? dataGrid;

    private readonly List<Person6GridModel> items = [];

    protected override void OnInitialized()
    {
        if (items.Count == 0)
        {
            var faker = new Faker<Person6Model>();
            faker.RuleFor(property: p => p.Id, setter: _ => Guid.NewGuid());
            faker.RuleFor(property: p => p.FirstName, setter: f => f.Person.FirstName);
            faker.RuleFor(property: p => p.LastName, setter: f => f.Person.LastName);
            faker.RuleFor(property: p => p.DateOfBirth, setter: f => DateOnly.FromDateTime(dateTime: f.Person.DateOfBirth));
            faker.RuleFor(property: p => p.Income, setter: f => f.Random.Decimal(min: 20000, max: 80000));

            List<Person6Model> data = faker.Generate(count: 180);
            items.AddRange(collection: data.Select(selector: Person6GridModel.ToGridModel));
        }
    }

    private async Task OnRowSelected(IEnumerable<Person6GridModel> obj)
    {
        Person6GridModel? rowItem = obj.FirstOrDefault();
        if (rowItem != null)
        {
            var model = Person6FormModel.ToFormModel(instance: Person6GridModel.ToModel(instance: rowItem));
            model.SaveFormAction = () =>
                                   {
                                       Person6GridModel item = items.Single(predicate: x => x.Id == model.Id);
                                       item.FirstName = model.FirstName;
                                       item.LastName = model.LastName;
                                       item.Income = model.Income.ToString(format: "C0", provider: CultureInfo.CurrentCulture);
                                       item.DateOfBirth = model.DateOfBirth.ToString(format: "yyyy-MM-dd", provider: CultureInfo.CurrentCulture);
                                       return Task.CompletedTask;
                                   };

            var page = dataGrid?.CurrentPageNumber ?? 0;

            await sidePanelService
                  .OpenInEditMode(model: model)
                  .ConfigureAwait(continueOnCapturedContext: false);

            if (dataGrid is not null)
            {
                dataGrid.GoToPage(page);
            }
        }
    }
}

public sealed class Person6Model
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; } = string.Empty;
    public required string LastName { get; set; } = string.Empty;
    public required DateOnly DateOfBirth { get; set; } = DateOnly.MinValue;
    public required decimal Income { get; set; } = decimal.Zero;
    public override string ToString() => $"{FirstName} {LastName}";
}

[UIGridClass(ResourceType = typeof(Example), AllowSorting = true)]
public sealed class Person6GridModel
{
    public Guid Id { get; set; }

    [UIGridField(Name = "FirstName")]
    public required string FirstName { get; set; }

    [UIGridField(Name = "LastName")]
    public required string LastName { get; set; }

    [UIGridField(Name = "DateOfBirth")]
    public required string DateOfBirth { get; set; }

    [UIGridField(Name = "Income")]
    public required string Income { get; set; }

    public static Person6GridModel ToGridModel([NotNull] Person6Model instance)
    {
        return new Person6GridModel
               {
                   Id = instance.Id
                 , FirstName = instance.FirstName
                 , LastName = instance.LastName
                 , Income = instance.Income.ToString(format: "C0", provider: CultureInfo.CurrentCulture)
                 , DateOfBirth = instance.DateOfBirth.ToString(format: "yyyy-MM-dd", provider: CultureInfo.CurrentCulture)
               };
    }

    public static Person6Model ToModel([NotNull] Person6GridModel instance) => new Person6Model
                                                                               {
                                                                                   Id = instance.Id
                                                                                 , FirstName = instance.FirstName
                                                                                 , LastName = instance.LastName
                                                                                 , Income = decimal.TryParse(s: instance.Income, style: NumberStyles.Currency, provider: CultureInfo.CurrentCulture, result: out decimal income)
                                                                                                ? income
                                                                                                : decimal.Zero
                                                                                 , DateOfBirth = DateOnly.TryParse(s: instance.DateOfBirth, provider: CultureInfo.CurrentCulture, result: out DateOnly dateOfBirth)
                                                                                                     ? dateOfBirth
                                                                                                     : DateOnly.MinValue
                                                                               };
}

[UIFormClass(title: "Person", ResourceType = typeof(Example))]
public sealed class Person6FormModel : FormModel
{
    private const string ColumnGroup1 = "PropertyColumn1";
    private const string ColumnGroup2 = "PropertyColumn2";
    private const string ColumnGroup3 = "PropertyColumn3";
    private const string ColumnGroup4 = "PropertyColumn4";

    public Person6FormModel()
    {
        AddViewColumn(name: ColumnGroup1, width: 12, order: 1, offset: 0);
        AddViewColumn(name: ColumnGroup2, width: 6, order: 1, offset: 0);
        AddViewColumn(name: ColumnGroup3, width: 6, order: 1, offset: 0);
        AddViewColumn(name: ColumnGroup4, width: 6, order: 1, offset: 0);
    }

    public Guid Id { get; set; }

    [UIFormField(name: "FirstName", ColumnGroup = ColumnGroup1)]
    public required string FirstName { get; set; }

    [UIFormField(name: "LastName", ColumnGroup = ColumnGroup1)]
    public required string LastName { get; set; }

    [UIFormField(name: "DateOfBirth", ColumnGroup = ColumnGroup2, FormParameters = ["DateFormat=yyyy-MM-dd"], DisplayParameters = ["Format={0:dddd d MMMM yyyy}"])]
    public required DateTime DateOfBirth { get; set; }

    [UIFormField(name: "Age", ColumnGroup = ColumnGroup3)]
    public int Age => CalculateAge(dateOfBirth: DateOfBirth);

    [UIFormField(name: "Income", ColumnGroup = ColumnGroup4, FormParameters = ["Format=C0"], DisplayParameters = ["Format=C0"])]
    public required decimal Income { get; set; }

    public static int CalculateAge(DateTime dateOfBirth)
    {
        DateTime today = DateTime.Today;
        int result = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(value: -result))
        {
            result--;
        }

        return result;
    }

    public static Person6FormModel ToFormModel([NotNull] Person6Model instance)
    {
        return new Person6FormModel
               {
                   Id = instance.Id
                 , FirstName = instance.FirstName
                 , LastName = instance.LastName
                 , Income = instance.Income
                 , DateOfBirth = instance.DateOfBirth.ToDateTime(time: TimeOnly.MinValue)
               };
    }
}
