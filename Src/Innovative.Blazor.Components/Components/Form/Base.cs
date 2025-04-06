namespace Innovative.Blazor.Components.Components.Form;

public interface IBaseFormModel
{
    /// <summary>
    /// The name of the form component.
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
}

public abstract class DisplayFormModel : IBaseFormModel
{
    public string? Name { get; set; }
    public object? Value { get; set; }
    public string? CssClass { get; set; }
    public ICollection<Column> ViewColumns { get; private set; } = new List<Column>();
    
    public void AddViewColumn(string name, int order, int width, int offset)
    {
        ViewColumns.Add(new Column
        {
            Name = name,
            Order = order,
            Width = width,
            Offset = offset
        });
        ViewColumns = ViewColumns.OrderBy(c => c.Order).ToList();
    }
}

public class Column
{
    /// <summary>
    /// the name of the column
    /// </summary>
    public string? Name { get; set; }
    
    public bool ShowOuterBorder { get; set; }
    
    /// <summary>
    /// The column order (lower values appear first)
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The column width as a percentage or CSS value (e.g. "50%", "300px")
    /// </summary>
    public int Width { get; set; } = 1;
    
    /// <summary>
    /// The offset of the column to add space before it
    /// </summary>
    public int Offset { get; set; }
}
