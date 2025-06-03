using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Innovative.Blazor.Components.Components;

public abstract class FormModel
{
    protected Collection<Column> ViewColumns { get; } = [];

    public Dictionary<string,string> Exceptions { get; } = [];

    /// <summary>
    /// The name (used as caption or label) of the form component.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The value of the form component.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The CSS class of the form component.
    /// </summary>
    public string? CssClass { get; set; }

    [UIFormViewAction(name: "Save", Order = 1)]
    public Func<Task>? SaveFormAction { get; set; }

    [UIFormViewAction(name: "Cancel", Order = 1)]
    public Func<Task>? CancelFormAction { get; set; }

    [UIFormViewAction(name: "Delete", Order = 1)]
    public Func<Task>? DeleteFormAction { get; set; }

    public IEnumerable<Column> Columns => ViewColumns.OrderBy(c => c.Order);

    public void AddViewColumn(string? name, int order, int width, int offset)
    {
        ViewColumns.Add(new Column
        {
            Name = name,
            Order = order,
            Width = width,
            Offset = offset
        });
    }

    public void AddException(Exception exception)
    {
        try
        {

            Debug.Assert(exception != null, nameof(exception) + " != null");
            var errors = exception!.Data["errors"];
                if (errors is not null && errors is IEnumerable<string> errorList)
                {
                    foreach (var error in errorList)
                    {
                        if (!Exceptions.ContainsKey(error))
                        {
                            Exceptions.TryAdd(error, error);
                        }
                    }
                }
                else
                {
                    if (!Exceptions.ContainsKey(exception.Message))
                    {
                        Exceptions.Add("General", exception.Message);
                    }
                }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void ClearExceptions()
    {
        Exceptions.Clear();
    }
}
