using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Innovative.Blazor.Components.Components;

public abstract class FormModel
{
    private readonly List<string> exceptions = [];
    protected Collection<Column> ViewColumns { get; } = [];

    public IEnumerable<string> Exceptions => exceptions;

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

    public async Task AddExceptionAsync(Exception exception)
    {
        Debug.Assert(exception != null, nameof(exception) + " != null");
        var exceptionName = exception.GetType().Name;

        if (exceptionName == "MicrosoftAspNetCoreMvcProblemDetails" || exceptionName.StartsWith("ProblemDetails",StringComparison.InvariantCulture ) )
        {
            PropertyInfo? additionalDataProp = exception.GetType().GetProperty("AdditionalData");
            //additionalData is a dictionary of string keys and object values
            //this should be use the get the errors


            if (additionalDataProp != null)
            {
                if (additionalDataProp.GetValue(exception) is IDictionary<string, object> additionalData)
                {

                    // Check if the additionalData contains an "errors" key and cast object as Erros class
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    if (additionalData.TryGetValue("errors", out dynamic errorsObj))
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    {
                        var serializedData = await KiotaJsonSerializer.SerializeAsStringAsync(errorsObj);

                        var root = JsonNode.Parse(serializedData); // of JsonDocument, zie alternatief onderaan

                        if (root is JsonObject obj)
                        {
                            foreach (var kvp in obj)
                            {
                                if (kvp.Value is JsonArray arr)
                                {
                                    var errorString = string.Empty;
                                    foreach (var item in arr)
                                    {
                                        if (item is JsonValue value)
                                        {
                                            errorString += value.ToString() + "<br/> ";
                                        }
                                    }
                                    exceptions.Add(errorString);
                                }
                            }
                        }

                        else
                        {
                            foreach (var kvp in additionalData)
                            {
                                exceptions.Add(kvp.Value.ToString()!);
                            }
                            // Check if the additionalData contains an "errors" key and cast object as Erros class
                        }
                    }
                }
            }
        }
        else if (exceptionName.Equals("ErrorResponse", StringComparison.OrdinalIgnoreCase))
        {
            PropertyInfo? additionalDataProp = exception.GetType().GetProperty("Errors");
            var values = additionalDataProp?.GetValue(exception) as dynamic;
            //get the AdditionalData property from the values
            var additionalData = values?.AdditionalData;

            var serializedData =   await KiotaJsonSerializer.SerializeAsStringAsync(additionalData);
            var root = JsonNode.Parse(serializedData); // of JsonDocument, zie alternatief onderaan
            
            if (root is JsonObject obj)
            {
                foreach (var kvp in obj)
                {
                    if (kvp.Value is JsonArray arr)
                    {
                        var errorString = string.Empty;
                        foreach (var item in arr)
                        {
                            if (item is JsonValue value)
                            {
                                errorString += value.ToString() + "<br/> ";
                            }
                        }
                        exceptions.Add(errorString);
                    }
                }
            }

        }
        else
        {
            exceptions.Add(exception.Message);
        }
    }
    public void AddException(string key, string message) => exceptions.Add(message);

    public void ClearExceptions()
    {
        exceptions.Clear();
    }
}

public class ErrorResponseDetails
{
    public Dictionary<string, object>? AdditionalData { get;  } 
}

