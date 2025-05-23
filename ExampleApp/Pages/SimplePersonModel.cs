using Innovative.Blazor.Components.Components;

namespace ExampleApp.Pages;

[UIFormClass(title: "Simple person example")]
public class SimplePersonModel : FormModel
{
    private const string NameColumn = "Name";
    private const string DataColumn = "Data";
    private const string InfoColumn = "Info";

    public SimplePersonModel()
    {
        AddViewColumn(name: NameColumn, order: 1, width: 2, offset: 0);
        AddViewColumn(name: DataColumn, order: 2, width: 2, offset: 0);
        AddViewColumn(name: InfoColumn, order: 3, width: 4, offset: 0);
    }

    [UIFormField(name: "First name", ColumnGroup = NameColumn)]
    public string? FirstName { get; set; }

    [UIFormField(name: "Last name", ColumnGroup = NameColumn)]
    public string? LastName { get; set; }

    [UIFormField(name: "Is active", ColumnGroup = DataColumn)]
    public bool IsActive { get; set; }

    [UIFormField(name: "Birth date", ColumnGroup = DataColumn)]
    public DateTime? BirthDate { get; set; }

    [UIFormField(name: "Biography", UseWysiwyg = true, ColumnGroup = InfoColumn)]
    public string? Biography { get; set; }
}
