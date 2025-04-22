using ExampleApp.Extensions;
using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Pages;

public partial class ExampleDisplayView : ComponentBase
{
    private readonly SimplePersonModel person = new SimplePersonModel()
                                                {
                                                    FirstName = "John",
                                                    LastName = "Doe",
                                                    IsActive = true,
                                                    BirthDate = new DateTime(1993, 5, 12)
                                                };
}

public class SimplePersonModel :DisplayFormModel
{
    public SimplePersonModel()
    {
        this.AddViewColumn("EmployeeInfo", 2,2,0);
        this.AddViewColumn("Description", 1, 1, 0);
    }
    [UIFormField(name: "First Name", ColumnGroup = "Name")]
    public string? FirstName { get; set; }

    [UIFormField(name: "Last Name", ColumnGroup = "LastName")]
    public string? LastName { get; set; }

    [UIFormField(name: "Is Active", DisplayComponent = typeof(CustomBoolStyle), ColumnGroup = "EmployeeInfo")]
    public bool IsActive { get; set; }

    [UIFormField(name: "Birth Date", ColumnGroup = "EmployeeInfo")]
    public DateTime? BirthDate { get; set; }

    [UIFormField(name: "Description", UseWysiwyg = true, ColumnGroup = "Description")]
    public string? Description { get; set; }
}
