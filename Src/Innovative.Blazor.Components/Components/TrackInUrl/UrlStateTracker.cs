using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Innovative.Blazor.Components.Components.TrackInUrl;

internal abstract class UrlStateTracker(NavigationManager navigationManager) : ComponentBase, IDisposable
{
    private readonly NavigationManager _navigationManager = navigationManager;

    private readonly Dictionary<PropertyInfo, object?> _trackedProperties = new ();

    private bool _isInitialized;
    private bool _isUpdatingFromUrl;

    public void Dispose()
    {
        _navigationManager.LocationChanged -= HandleLocationChanged!;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        var trackedProps = GetType()
            .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
            .Where(predicate: p => p.GetCustomAttribute<TrackInUrlAttribute>() != null)
            .ToList();

        foreach (var prop in trackedProps)
        {
            _trackedProperties[key: prop] = prop.GetValue(obj: this)!;
        }

        _navigationManager.LocationChanged += HandleLocationChanged!;
        ReadFromUrl();

        _isInitialized = true;
    }

    private void HandleLocationChanged(object sender, LocationChangedEventArgs e)
    {
        ReadFromUrl();
    }

    /// <summary>
    ///     Called when a tracked property is changed.
    /// </summary>
    protected void OnPropertyPossiblyChanged()
    {
        if (!_isInitialized || _isUpdatingFromUrl)
            return;

        UpdateUrlIfNeeded();
    }

    /// <summary>
    ///     Checks if any tracked property changed.
    /// </summary>
    private void UpdateUrlIfNeeded()
    {
        var anyChange = false;
        var updatedState = new Dictionary<string, object?>();

        foreach (var kvp in _trackedProperties)
        {
            var prop = kvp.Key;
            var oldValue = kvp.Value;
            var newValue = prop.GetValue(obj: this);

            updatedState[key: prop.Name] = newValue;

            if (!Equals(objA: oldValue, objB: newValue))
            {
                anyChange = true;
                _trackedProperties[key: prop] = newValue;
            }
        }

        if (!anyChange)
            return;

        var json = JsonSerializer.Serialize(value: updatedState);
        var base64 = Convert.ToBase64String(inArray: Encoding.UTF8.GetBytes(s: json));

        var currentUri = _navigationManager.Uri;
        var newUri = UpdateQueryStringParameter(url: currentUri, key: "dataset", value: base64);

        _navigationManager.NavigateTo(uri: newUri, forceLoad: false);
    }

    /// <summary>
    ///     Reads the state from the URL and updates the properties.
    /// </summary>
    private void ReadFromUrl()
    {
        _isUpdatingFromUrl = true;
        try
        {
            var uri = _navigationManager.ToAbsoluteUri(relativeUri: _navigationManager.Uri);
            var query = QueryHelpers.ParseQuery(queryString: uri.Query);

            if (query.TryGetValue(key: "dataset", value: out var base64Values))
            {
                var base64 = base64Values.ToString();
                var json = Encoding.UTF8.GetString(bytes: Convert.FromBase64String(s: base64));
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json: json);

                if (dict != null)
                {
                    foreach (var prop in _trackedProperties.Keys.ToList())
                    {
                        if (dict.TryGetValue(key: prop.Name, value: out var jsonElement))
                        {
                            var typedValue = JsonSerializer.Deserialize(json: jsonElement.GetRawText(), returnType: prop.PropertyType);
                            prop.SetValue(obj: this, value: typedValue);
                            _trackedProperties[key: prop] = typedValue;
                        }
                    }
                    StateHasChanged();
                }
            }
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            Console.WriteLine(value: $"Error reading state from URL: {ex.Message}");
        }
        finally
        {
            _isUpdatingFromUrl = false;
        }
    }

    /// <summary>
    ///     Helper to replace or key in a query string.
    /// </summary>
    private static string UpdateQueryStringParameter(string url, string key, string value)
    {
        if (!Uri.TryCreate(uriString: url, uriKind: UriKind.Absolute, result: out var uri))
            return url;

        var baseUri = uri.GetLeftPart(part: UriPartial.Path);
        var queryParams = QueryHelpers.ParseQuery(queryString: uri.Query);
        var updatedParams = new Dictionary<string, StringValues>();

        foreach (var qp in queryParams)
        {
            if (qp.Key.Equals(value: key, comparisonType: StringComparison.OrdinalIgnoreCase))
                continue;

            updatedParams[key: qp.Key] = qp.Value;
        }

        updatedParams[key: key] = value;

        var newQuery = QueryString.Create(parameters: updatedParams);

        return baseUri + newQuery;
    }
}