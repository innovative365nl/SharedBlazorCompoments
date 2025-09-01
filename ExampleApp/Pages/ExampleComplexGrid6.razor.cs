using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ExampleApp.Translations;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;

namespace ExampleApp.Pages;

public partial class ExampleComplexGrid6(IInnovativeSidePanelService sidePanelService)
{
    private readonly List<Person6GridModel> items = [];

    protected override void OnInitialized()
    {
        if (items.Count == 0)
        {
            ExampleDataSet.Instance.GenerateTestData(amount: 180);
            items.AddRange(collection: ExampleDataSet.Instance.Data.Select(selector: Person6GridModel.ToGridModel));
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
                                       item.Income = model.Income is null
                                                         ? string.Empty
                                                         : model.Income.Value.ToCurrencyString();
                                       item.DateOfBirth = model.DateOfBirth is null
                                                              ? string.Empty
                                                              : model.DateOfBirth.Value.ToDateString();
                                       item.EmailAddress = model.EmailAddress;
                                       item.Phone = model.Phone;
                                       return Task.CompletedTask;
                                   };

            await sidePanelService
                  .OpenInEditMode(model: model)
                  .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
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

    public string? EmailAddress { get; set; }

    public string? Phone { get; set; }

    public static Person6GridModel ToGridModel([NotNull] PersonModel instance)
    {
        return new Person6GridModel
               {
                   Id = instance.Id
                 , FirstName = instance.FirstName
                 , LastName = instance.LastName
                 , Income = instance.Income.ToCurrencyString()
                 , DateOfBirth = instance.DateOfBirth.ToDateString()
                 , EmailAddress = instance.EmailAddress
                 , Phone = instance.Phone
               };
    }

    public static PersonModel ToModel([NotNull] Person6GridModel instance) => new PersonModel
                                                                              {
                                                                                  Id = instance.Id
                                                                                , FirstName = instance.FirstName
                                                                                , LastName = instance.LastName
                                                                                , Income = instance.Income.ToCurrency()
                                                                                , DateOfBirth = instance.DateOfBirth.ToDate()
                                                                                , EmailAddress = instance.EmailAddress
                                                                                , Phone = instance.Phone
                                                                              };
}

[UIFormClass(title: "Person", ResourceType = typeof(Example))]
public sealed class Person6FormModel : FormModel
{
    private const string ColumnGroup1 = "PropertyColumn1";
    private const string ColumnGroup2 = "PropertyColumn2";
    private const string ColumnGroup3 = "PropertyColumn3";
    private const string ColumnGroup4 = "PropertyColumn4";
    private const string ColumnGroup5 = "PropertyColumn5";

    public Person6FormModel()
    {
        AddViewColumn(name: ColumnGroup1, width: 12, order: 1, offset: 0);
        AddViewColumn(name: ColumnGroup2, width: 6, order: 1, offset: 0);
        AddViewColumn(name: ColumnGroup3, width: 6, order: 1, offset: 0);
        AddViewColumn(name: ColumnGroup4, width: 6, order: 1, offset: 0);
        AddViewColumn(name: ColumnGroup5, width: 12, order: 1, offset: 0);
    }

    public Guid Id { get; set; }

    [UIFormField(name: "FirstName", ColumnGroup = ColumnGroup1)]
    [Required]
    [MinLength(2)]
    [MaxLength(24)]
    public required string FirstName { get; set; }

    [UIFormField(name: "LastName", ColumnGroup = ColumnGroup1)]
    [Required]
    [MinLength(2)]
    [MaxLength(48)]
    public required string LastName { get; set; }

    [UIFormField(name: "DateOfBirth", ColumnGroup = ColumnGroup2, FormParameters = [$"DateFormat={Constants.DateFormat}"], DisplayParameters = ["Format={0:dddd d MMMM yyyy}"])]
    [Required]
    public required DateTime? DateOfBirth { get; set; }

    [UIFormField(name: "Age", ColumnGroup = ColumnGroup3)]
    public int Age => DateOfBirth?.Age() ?? 0;

    [UIFormField(name: "Income", ColumnGroup = ColumnGroup4, FormParameters = [$"Format={Constants.CurrencyFormat}"], DisplayParameters = [$"Format={Constants.CurrencyFormat}"])]
    public decimal? Income { get; set; }

    [UIFormField(name: "Email", ColumnGroup = ColumnGroup5)]
    [Required]
    [EmailAddress(ErrorMessageResourceType = typeof(Example), ErrorMessageResourceName = "EmailAddressValidation")]
    public string? EmailAddress { get; set; }

    [UIFormField(name: "Phone", ColumnGroup = ColumnGroup5)]
    [Phone(ErrorMessageResourceType = typeof(Example), ErrorMessageResourceName = "PhoneNumberValidation")]
    public string? Phone { get; set; }
    
    public static Person6FormModel ToFormModel([NotNull] PersonModel instance)
    {
        return new Person6FormModel
               { Id = instance.Id
               , FirstName = instance.FirstName
               , LastName = instance.LastName
               , Income = instance.Income
               , DateOfBirth = instance.DateOfBirth.ToDateTime(time: TimeOnly.MinValue)
               , EmailAddress = instance.EmailAddress
               , Phone = instance.Phone
               };
    }
}
