namespace Innovative.Blazor.Components.Components;

public interface IFormModel
{
    /// <summary>
    /// The name (used as caption or label) of the form component.
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    /// The value of the form component.
    /// </summary>
    object? Value { get; set; }

    /// <summary>
    /// The CSS class of the form component.
    /// </summary>
    string? CssClass { get; set; }

    IEnumerable<Column> Columns { get; }
}

public abstract class FormModelBase : IFormModel
{
    protected readonly List<Column> columns = [];

    public string? Name { get; set; }
    public object? Value { get; set; }
    public string? CssClass { get; set; }

    public IEnumerable<Column> Columns => columns.OrderBy(c => c.Order);


}

public abstract class DisplayFormModel : FormModelBase
{
    public void AddViewColumn(string name, int order, int width, int offset)
    {
        columns.Add(new Column
        {
            Name = name,
            Order = order,
            Width = width,
            Offset = offset
        });
    }
}

public abstract class EditFormModel : FormModelBase
{
    public void AddEditColumn(string name, int order, int width, int offset)
    {
        columns.Add(new Column
                    {
                        Name = name,
                        Order = order,
                        Width = width,
                        Offset = offset
                    });
    }

    public Action? SaveAction;
    public Action? DeleteAction;
}
