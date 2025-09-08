using ExampleApp.Components;
using ExampleApp.Translations;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;

namespace ExampleApp.Pages;

public partial class ExampleComplexGrid4(IInnovativeSidePanelService sidePanelService)
{
    private readonly List<Person4GridModel> items = [];

    protected override void OnInitialized()
    {
        ExampleDataSet.Instance.GenerateTestData(amount: 10);
        items.AddRange(collection: ExampleDataSet.Instance.Data.Select(selector: Person4GridModel.ToGridModel));
    }

    private async Task OnRowSelected(IEnumerable<Person4GridModel> obj)
    {
        Person4GridModel? rowItem = obj.FirstOrDefault();
        if (rowItem is null)
            return;

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

[UIGridClass(ResourceType = typeof(Example), AllowSorting = true)]
public sealed class Person4GridModel
{
    public Guid Id { get; set; }

    [UIGridField(Name = nameof(Example.FirstName))]
    public string? FirstName { get; set; }

    [UIGridField(Name = nameof(Example.LastName))]
    public string? LastName { get; set; }

    public static Person4GridModel ToGridModel(PersonModel instance)
    {
        ArgumentNullException.ThrowIfNull(argument: instance);

        return new Person4GridModel
               { Id = instance.Id
               , FirstName = instance.FirstName
               , LastName = instance.LastName
               };
    }
    public static Person4Model ToModel(Person4GridModel instance)
    {
        ArgumentNullException.ThrowIfNull(argument: instance);

        return new Person4Model
               { Id = instance?.Id ?? Guid.NewGuid()
               , FirstName = instance?.FirstName
               , LastName = instance?.LastName
               };
    }
}

[UIFormClass(title: nameof(Example.Person), ResourceType = typeof(Example))]
public sealed class Person4FormModel : FormModel
{
    private const string ColumnGroup1 = "PropertyColumn1";

    public Person4FormModel() => AddViewColumn(name: ColumnGroup1, width: 12, order: 1, offset: 0);

    public Guid Id { get; set; }

    [UIFormField(name: nameof(Example.FirstName), ShouldNotifyChanges = true, ColumnGroup = ColumnGroup1)]
    public string? FirstName { get; init; }

    [UIFormField(name: nameof(Example.LastName), ShouldNotifyChanges = true, ColumnGroup = ColumnGroup1)]
    public string? LastName { get; init; }

    [UIFormField(name: nameof(Example.FullName), FormComponent = typeof(FullNamePreviewComponent), FormParameters = ["DisplayLabel=false"], ColumnGroup = ColumnGroup1)]
    public Person4Model? FullName { get; init; }

    public static Person4FormModel ToFormModel(Person4Model instance)
    {
        ArgumentNullException.ThrowIfNull(argument: instance);

        return new Person4FormModel
               {
                   Id = instance?.Id ?? Guid.NewGuid()
                 , FirstName = instance?.FirstName
                 , LastName = instance?.LastName
                 , FullName = instance
               };
    }
}
