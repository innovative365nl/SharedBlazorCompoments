using System.Collections.ObjectModel;

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
    public string? Name { get; set; }
    public object? Value { get; set; }
    public string? CssClass { get; set; }
    protected Collection<Column> ViewColumns { get; } = [];
    public IEnumerable<Column> Columns => ViewColumns.OrderBy(c => c.Order);
}

public abstract class DisplayFormModel : FormModelBase
{
    public void AddViewColumn(string name, int order, int width, int offset)
    {
        ViewColumns.Add(new Column
        {
            Name = name,
            Order = order,
            Width = width,
            Offset = offset
        });
    }
}
