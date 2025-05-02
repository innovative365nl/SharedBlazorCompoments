using ExampleApp.Extensions;
using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Pages;

public partial class ExampleDisplayView : ComponentBase
{
    private readonly SimplePersonModel person = new SimplePersonModel
    {
        FirstName = "John",
        LastName = "Doe",
        IsActive = true,
        BirthDate = new DateTime(1993, 5, 12),
        Biography = "This is a biography of John Doe. He is a software engineer with over 10 years of experience in the industry."
    };
}

public class SimplePersonModel :DisplayFormModel
{
    private const string NameColumn = "Name";
    private const string DataColumn = "Data";
    private const string InfoColumn = "Info";

    public SimplePersonModel()
    {
        this.AddViewColumn(NameColumn, 1,2,0);
        this.AddViewColumn(DataColumn, 2, 2, 0);
        this.AddViewColumn(InfoColumn, 3, 2, 0);
    }

    [UIFormField(name: "First Name", ColumnGroup = NameColumn)]
    public string? FirstName { get; set; }

    [UIFormField(name: "Last Name", ColumnGroup = NameColumn)]
    public string? LastName { get; set; }

    [UIFormField(name: "Is Active", DisplayComponent = typeof(CustomBooleanStyle), ColumnGroup = DataColumn)]
    public bool IsActive { get; set; }

    [UIFormField(name: "Birth Date", ColumnGroup = DataColumn)]
    public DateTime? BirthDate { get; set; }

    [UIFormField(name: "Biography", UseWysiwyg = true, ColumnGroup = InfoColumn)]
    public string? Biography { get; set; }
}
