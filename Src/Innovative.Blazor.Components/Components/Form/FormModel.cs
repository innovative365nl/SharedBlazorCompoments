using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Innovative.Blazor.Components.Components;

public sealed record FormAlert(Guid Id, AlertSeverity Severity, string Summary, string? Detail, bool Closable, bool ShowInForm, bool ShowInDetail);

public abstract class FormModel
{
    private readonly List<FormAlert> alerts = [];
    private readonly Dictionary<string, HashSet<string>> exceptions = [];
    public IReadOnlyList<FormAlert> Alerts => alerts;

    protected Collection<Column> ViewColumns { get; } = [];

    public IEnumerable<string> Exceptions => exceptions.SelectMany(x => x.Value);

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

    // Progress streaming support for long-running actions (e.g., Save)
    // Components can subscribe to display progress messages (like SSE output)
    public event EventHandler<ProgressEventArgs>? OnProgress;

    public void ReportProgress(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            OnProgress?.Invoke(this, new ProgressEventArgs(message));
        }
    }

    public Guid AddAlert
    (
        AlertSeverity severity
      , string message
      , bool closable = true
      , string? detail = null
      , bool inForm = true
      , bool inDetail = true
    )
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));
        var id = Guid.NewGuid();
        alerts.Add(new FormAlert(id, severity, message, detail, closable, inForm, inDetail));
        return id;
    }

    public void RemoveAlert(Guid id)
    {
        alerts.RemoveAll(a => a.Id == id);
    }

    public void ClearAlerts() => alerts.Clear();

    public void AddViewColumn
    (
        string? name
      , int order
      , int width
      , int offset
    )
    {
        ViewColumns.Add(new Column
                        {
                            Name = name
                          , Order = order
                          , Width = width
                          , Offset = offset
                        });
    }

    public async Task AddExceptionAsync(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var exceptionName = exception.GetType().Name;

        if (IsProblemDetails(exceptionName))
        {
            AddExceptions(exceptionName
                        , await GetErrorMessagesFromAdditionalDataAsync(exception)
                              .ConfigureAwait(continueOnCapturedContext: false));
        }
        else if (IsErrorResponse(exceptionName))
        {
            AddExceptions(exceptionName
                        , await GetErrorMessagesFromErrorResponseAsync(exception)
                              .ConfigureAwait(continueOnCapturedContext: false));
        }
        else
        {
            AddException(exceptionName, exception.Message);
        }
    }

    private static bool IsProblemDetails(string exceptionName) => exceptionName.Equals(value: "MicrosoftAspNetCoreMvcProblemDetails", comparisonType: StringComparison.Ordinal)
                                                               || exceptionName.StartsWith(value: "ProblemDetails", comparisonType: StringComparison.InvariantCulture);

    private static bool IsErrorResponse(string exceptionName) => exceptionName.Equals(value: "ErrorResponse", comparisonType: StringComparison.OrdinalIgnoreCase);

    private static async Task<List<string>> GetErrorMessagesFromAdditionalDataAsync(Exception exception)
    {
        // additionalData is a dictionary of string keys and object values
        // this should be used to the get the errors
        PropertyInfo? additionalDataProp = exception.GetType().GetProperty("AdditionalData");

        if (additionalDataProp?.GetValue(exception) is not IDictionary<string, object> additionalData)
        {
            return [];
        }

        // Check if the additionalData contains an "errors" key and cast object as Errors class
        return additionalData.TryGetValue("errors", out dynamic? errorsObj)
                   ? await GetErrorMessagesAsync(errorsObj)
                   : additionalData.Select(kvp => $"{kvp.Key} = {kvp.Value}")
                                   .ToList();
    }

    private static async Task<List<string>> GetErrorMessagesFromErrorResponseAsync(Exception exception)
    {
        PropertyInfo? errorsProp = exception.GetType().GetProperty("Errors");
        dynamic? values = errorsProp?.GetValue(exception);
        var errorsObj = values?.AdditionalData;

        return await GetErrorMessagesAsync(errorsObj);
    }

    private static async Task<List<string>> GetErrorMessagesAsync(dynamic? errorsObj)
    {
        if (errorsObj is null)
            return [];

        string serializedData = await KiotaJsonSerializer.SerializeAsStringAsync(errorsObj);
        var root = JsonNode.Parse(serializedData);

        if (root is not JsonObject jsonObject)
            return [];

        List<string> result = [];
        foreach (KeyValuePair<string, JsonNode?> kvp in jsonObject)
        {
            if (kvp.Value is not JsonArray jsonArray)
                continue;

            foreach (var item in jsonArray)
            {
                if (item is JsonValue value)
                    result.Add(value.ToString());
            }
        }

        return result;
    }

    public void AddExceptions(string key, IEnumerable<string> messages)
    {
        var exceptionMessages = messages.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (exceptionMessages.Count == 0)
            return;

        foreach (var message in exceptionMessages)
        {
            AddException(key, message);
        }
    }

    public void AddException(string key, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (exceptions.TryGetValue(key, out var value))
        {
            value.Add(message);
            return;
        }

        exceptions[key] = new HashSet<string>();
        exceptions[key].Add(message);
    }

    public void RemoveException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        _ = exceptions.Remove(exception.GetType().Name);
    }
    public void RemoveException(string key) => _ = exceptions.Remove(key);
    public void ClearExceptions() => exceptions.Clear();
}
