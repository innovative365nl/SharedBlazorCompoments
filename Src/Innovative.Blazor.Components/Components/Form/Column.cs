namespace Innovative.Blazor.Components.Components;

public class Column
{
    /// <summary>
    /// The name of the column.
    /// </summary>
    public string? Name { get; set; }
    
    public bool ShowOuterBorder { get; set; }

    /// <summary>
    /// The order of the column.
    /// </summary>
    /// <remarks>Lower values appear first.</remarks>
    public int Order { get; set; }

    /// <summary>
    /// The column width as a percentage or CSS value (e.g. "50%", "300px").
    /// </summary>
    public int Width { get; set; } = 1;
    
    /// <summary>
    /// The offset of the column to add space before it.
    /// </summary>
    public int Offset { get; set; }
}
