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
        Debug.Assert(exception != null, nameof(exception) + " != null");

        if (exception.GetType().Name == "MicrosoftAspNetCoreMvcProblemDetails")
        {
            var additionalDataProp = exception.GetType().GetProperty("AdditionalData");
            if (additionalDataProp != null)
            {
                var additionalData = additionalDataProp.GetValue(exception) as IDictionary<string, object>;
                if (additionalData != null)
                {
                    if (additionalData.TryGetValue("errors", out var errorsObj) && errorsObj is IDictionary<string, string> errorsDict)
                    {
                        foreach (var kvp in errorsDict)
                        {
                            Exceptions.TryAdd(kvp.Key, kvp.Value ?? string.Empty);
                        }
                    }
                    else
                    {
                        foreach (var kvp in additionalData)
                        {
                            Exceptions.TryAdd(kvp.Key, kvp.Value?.ToString() ?? string.Empty);
                        }
                    }
                    return;
                }
            }
        }
    }

    public void AddException(string key, string message) => Exceptions.TryAdd(key, message);

    public void ClearExceptions()
    {
        Exceptions.Clear();
    }
}
