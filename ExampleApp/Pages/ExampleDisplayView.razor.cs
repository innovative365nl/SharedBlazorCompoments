using ExampleApp.Extensions;
using ExampleApp.Translations;
using Innovative.Blazor.Components.Components.Dialog;
using Innovative.Blazor.Components.Components.Form;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Pages;

public partial class ExampleDisplayView : ComponentBase
{
    public ExampleDisplayView()
    {
        _person = new SimplePersonModel()
        {
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            BirthDate = new DateTime(1993,
                5,
                12),
        };
    }

    private readonly SimplePersonModel _person;
}

[UIFormClass(title: nameof(Example.DialogService_Person),
    ResourceType = typeof(Example), ColumnOrder = new[] { "Name", "EmployeeInfo", "Description" },
    ColumnWidthNames = new[] { "Name", "EmployeeInfo", "Description" },
    ColumnWidthValues = new[] { 1, 1, 3 })]
public class SimplePersonModel :DisplayFormModel
{
    public SimplePersonModel()
    {
        this.AddViewColumn("employeeInfo", 1,2,0);
        this.AddViewColumn("description", 3, 0, 0);
    }
    [UIFormField(name: "First Name", ColumnGroup = "Name")]
    public string? FirstName { get; set; }

    [UIFormField(name: "Last Name", ColumnGroup = "Name")]
    public string? LastName { get; set; }

    // [UIFormField(name: "Age", ColumnGroup = "EmployeeInfo")]
    //public int? Age => DateTime.Now.Year - BirthDate!.Value.Year;

    [UIFormField(name: "Is Active", DisplayComponent = typeof(CustomBoolStyle), ColumnGroup = "EmployeeInfo")]
    public bool IsActive { get; set; }

    [UIFormField(name: "Birth Date", ColumnGroup = "EmployeeInfo")]
    public DateTime? BirthDate { get; set; }

    [UIFormField(name: "Description", UseWysiwyg = true, ColumnGroup = "Description")]
    public string? Description { get; set; }
//
//     [UIFormViewAction(Name = "Update Password", Order = 1)]
//     public  Action<int>? UpdatePasswordAction { get; set; }
//
//     [UIFormViewAction(Name = "Control Password", Order = 1, CustomComponent = typeof(PasswordUpdateComponent))]
//     public   Action<int>? ControlePasswordAction { get; set; }
}