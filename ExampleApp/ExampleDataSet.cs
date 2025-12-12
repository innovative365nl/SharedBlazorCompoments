using Bogus;
using Innovative.Blazor.Components.Common;
using Innovative.Blazor.Components.Components;

namespace ExampleApp;

public class ExampleDataSet
{
    public static readonly ExampleDataSet Instance = new ExampleDataSet();

    private ExampleDataSet() { }

    public IList<PersonModel> Data { get; } = [];

    public void GenerateTestData(int amount)
    {
        var faker = new Faker<PersonModel>();
        faker.RuleFor(property: p => p.Id, setter: _ => Guid.NewGuid());
        faker.RuleFor(property: p => p.FirstName, setter: f => f.Person.FirstName);
        faker.RuleFor(property: p => p.LastName, setter: f => f.Person.LastName);
        faker.RuleFor(property: p => p.DateOfBirth, setter: f => DateOnly.FromDateTime(dateTime: f.Person.DateOfBirth));
        faker.RuleFor(property: p => p.Income, setter: f => f.Random.Decimal(min: 20000, max: 80000));
        faker.RuleFor(property: p => p.EmailAddress, setter: f => f.Person.Email);
        faker.RuleFor(property: p => p.Phone, setter: f => f.Person.Phone);
        faker.RuleFor(property: p => p.HomePage, setter: f => f.Internet.Url());

        var data = faker.Generate(count: amount);
        Data.Clear();
        Data.AddRange(data);
    }
}

public sealed record PersonModel
{
    public Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required decimal Income { get; init; }
    public string? EmailAddress { get; init; }
    public string? Phone { get; init; }
    public string? HomePage { get; init; }
    public override string ToString() => $"{FirstName} {LastName}";
}

public class ExampleDataItem
{
    [UIGridField]
    public int Id { get; set; }

    [UIGridField]
    public string Name { get; set; } = string.Empty;

    [UIGridField]
    public decimal Value { get; set; }

    [UIGridField]
    public DateTime Date { get; set; }

    public static IReadOnlyCollection<ExampleDataItem> GetExampleData()
    {
        return new List<ExampleDataItem>
               {
                   new ExampleDataItem
                   {
                       Id = 1
                     , Name = "Alpha"
                     , Value = 10.5m
                     , Date = DateTime.Today
                   }
                 , new ExampleDataItem
                   {
                       Id = 2
                     , Name = "Beta"
                     , Value = 20.0m
                     , Date = DateTime.Today.AddDays(-1)
                   }
                 , new ExampleDataItem
                   {
                       Id = 3
                     , Name = "Gamma"
                     , Value = 30.75m
                     , Date = DateTime.Today.AddDays(-2)
                   }
               };
    }
}
